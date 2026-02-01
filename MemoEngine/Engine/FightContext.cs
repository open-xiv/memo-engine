using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MemoEngine.Models;
using MemoEngine.Models.Events;
using Action = MemoEngine.Models.Action;


namespace MemoEngine.Engine;

internal class FightContext
{
    // duty config
    private readonly DutyConfig dutyConfig;

    #region Payload

    // time
    private DateTimeOffset  startTime;
    private DateTimeOffset? lastCombatOptOutTime;

    // progress
    private bool isClear;
    private int  phaseIndex;
    private int  subphaseIndex; // checkpoint index

    // enemy
    private double enemyHp;

    #endregion

    #region DutyState

    // players
    private readonly ConcurrentDictionary<uint, PlayerPayload> players = [];

    // variables
    private readonly ConcurrentDictionary<string, object?> variables       = [];
    private readonly ListenerManager                       listenerManager = new();

    #endregion

    #region Lifecycle

    private readonly Context context;

    public FightContext(Context context, DutyConfig dutyConfig)
    {
        this.context    = context;
        this.dutyConfig = dutyConfig;
        ResetState();
        context.Lifecycle = EngineState.WaitingStart;
    }

    #endregion

    #region EventProcess

    public void ProcessEvent(IEvent e)
    {
        // lifecycle related events
        LifecycleEvent(e);

        if (context.Lifecycle is not EngineState.Recording)
            return;

        // specific events
        switch (e)
        {
            // death
            case PlayerDied death when players.TryGetValue(death.EntityId, out var player):
                player.DeathCount++;
                break;

            // enemy hp change
            case CombatantHpUpdated hpChanged when hpChanged.DataId == context.EnemyDataId:
                enemyHp = hpChanged.MaxHp == 0 ? 1 : hpChanged.CurrentHp / hpChanged.MaxHp;
                break;
        }

        // listeners
        var relatedListener = listenerManager.FetchListeners(e);
        foreach (var listener in relatedListener)
        {
            if (CheckTrigger(listener.Trigger, e))
                EmitMechanic(listener.Mechanic);
        }
    }

    public void LifecycleEvent(IEvent e)
    {
        switch (e)
        {
            case CombatOptIn st:
                if (context.Lifecycle is EngineState.WaitingStart)
                {
                    ResetState();
                    StartSnap(st.PartyPlayers);
                    context.Lifecycle = EngineState.Recording;
                }
                break;

            case CombatOptOut:
                lastCombatOptOutTime = DateTime.UtcNow;
                break;

            case DutyWiped:
                isClear = false;
                CompletedSnap();
                context.Lifecycle = EngineState.WaitingStart;

                break;

            case DutyCompleted:
                isClear = true;
                CompletedSnap();
                context.Lifecycle = EngineState.WaitingStart;
                break;
        }
    }

    #endregion

    #region Snapshot

    private void StartSnap(IReadOnlyDictionary<uint, PlayerPayload>? partyPlayers)
    {
        // time
        startTime            = DateTimeOffset.UtcNow;
        lastCombatOptOutTime = null;

        // enemy hp
        enemyHp = 1.0;

        // death count
        players.Clear();
        if (partyPlayers is not null)
        {
            foreach (var kvp in partyPlayers)
                players.TryAdd(kvp.Key, kvp.Value);
        }
    }

    private void CompletedSnap()
    {
        // time
        var endTime = lastCombatOptOutTime ?? DateTime.UtcNow;
        endTime = endTime > startTime ? endTime : DateTime.UtcNow;
        var duration = (endTime - startTime).Ticks * 100;

        // progress
        var progress = new FightProgressPayload
        {
            PhaseId    = (uint)Math.Max(phaseIndex, 0),
            SubphaseId = (uint)Math.Max(subphaseIndex, 0),
            EnemyId    = context.EnemyDataId,
            EnemyHp    = enemyHp
        };

        // payload
        var payload = new FightRecordPayload
        {
            StartTime = startTime,
            Duration  = duration,
            ZoneId    = dutyConfig.ZoneId,
            Players   = players.Values.Select(p => p.Clone()).ToList(),
            IsClear   = isClear,
            Progress  = progress
        };

        // notify
        _ = Task.Run(() => context.RaiseFightFinalized(payload));
    }

    #endregion

    #region StateMachine

    private void ResetState()
    {
        // progress
        isClear       = false;
        phaseIndex    = 0;
        subphaseIndex = -1;

        // enemy
        context.EnemyDataId = 0;

        // init listeners & variables
        listenerManager.Clear();
        variables.Clear();
        foreach (var vars in dutyConfig.Variables)
            variables[vars.Name] = vars.Initial;

        // enter start phase
        EnterPhase(0);
    }

    private void EnterPhase(int phaseId)
    {
        // phase transition
        var phase = dutyConfig.Timeline.Phases[phaseId];
        phaseIndex    = phaseId;
        subphaseIndex = -1;

        // enemy
        context.EnemyDataId = phase.TargetId;

        // clear triggers
        listenerManager.Clear();

        // mechanics
        // from checkpoints
        var mechanics = new HashSet<string>(phase.CheckpointNames);
        // from transitions
        foreach (var transition in phase.Transitions)
        {
            foreach (var condition in transition.Conditions)
            {
                if (condition.Type != "MECHANIC_TRIGGERED")
                    continue;
                mechanics.Add(condition.MechanicName);
            }
        }

        // register listeners
        foreach (var mechanic in dutyConfig.Mechanics.Where(m => mechanics.Contains(m.Name)))
            listenerManager.Register(new ListenerState(mechanic, mechanic.Trigger));

        // enemy
        context.EnemyDataId = phase.TargetId;
    }

    private void EmitMechanic(Mechanic mechanic)
    {
        // update progress
        var phase            = dutyConfig.Timeline.Phases[phaseIndex];
        var newSubphaseIndex = phase.CheckpointNames.IndexOf(mechanic.Name);
        if (newSubphaseIndex >= subphaseIndex)
            subphaseIndex = newSubphaseIndex;

        // emit event
        foreach (var action in mechanic.Actions)
            EmitAction(action);

        // check transition
        CheckTransition(mechanic);
    }

    private void EmitAction(Action action)
    {
        // update variables
        switch (action.Type)
        {
            case "INCREMENT_VARIABLE":
                if (variables.TryGetValue(action.Name, out var val) && val is long or int)
                    variables[action.Name] = Convert.ToInt64(val) + 1;
                break;
            case "SET_VARIABLE":
                variables[action.Name] = action.Value;
                break;
        }

        // check transition
        CheckTransition(action.Name);
    }

    private void CheckTransition(Mechanic mechanic)
    {
        var phase = dutyConfig.Timeline.Phases[phaseIndex];
        foreach (var transition in phase.Transitions)
        {
            if (transition.Conditions
                          .Where(x => x.Type == "MECHANIC_TRIGGERED")
                          .Any(x => x.MechanicName == mechanic.Name))
            {
                EnterPhase(dutyConfig.Timeline.Phases.FindIndex(x => x.Name == transition.TargetPhase));
                return;
            }
        }
    }

    private void CheckTransition(string variable)
    {
        var phase = dutyConfig.Timeline.Phases[phaseIndex];
        foreach (var transition in phase.Transitions)
        {
            if (transition.Conditions
                          .Where(x => x.Type == "EXPRESSION")
                          .Any(x => x.Expression.Contains(variable) && CheckExpression(x.Expression)))
            {
                EnterPhase(dutyConfig.Timeline.Phases.FindIndex(x => x.Name == transition.TargetPhase));
                return;
            }
        }
    }

    private static bool CheckTrigger(Trigger trigger, IEvent? e = null)
    {
        switch (trigger.Type)
        {
            case "ACTION_EVENT":
                if (e is IActionEvent actionEvent)
                    return EventTools.Match(actionEvent, trigger);
                return false;
            case "COMBATANT_EVENT":
                if (e is ICombatantEvent combatantEvent)
                    return EventTools.Match(combatantEvent, trigger);
                return false;
            case "STATUS_EVENT":
                if (e is IStatusEvent statusEvent)
                    return EventTools.Match(statusEvent, trigger);
                return false;
            default:
                return false;
        }
    }

    private bool CheckExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var parts = expression.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        var variablePath    = parts[0];
        var op              = parts[1];
        var literalValueStr = parts[2];

        if (!variablePath.StartsWith("variables."))
            return false;
        var variableName = variablePath.Substring("variables.".Length);

        if (!variables.TryGetValue(variableName, out var currentValueObj))
            return false;

        try
        {
            var currentValue = Convert.ToDouble(currentValueObj);
            var targetValue  = Convert.ToDouble(literalValueStr);

            return op switch
            {
                "==" => Math.Abs(currentValue - targetValue) < 0.05,
                "!=" => Math.Abs(currentValue - targetValue) > 0.05,
                ">" => currentValue > targetValue,
                ">=" => currentValue >= targetValue,
                "<" => currentValue < targetValue,
                "<=" => currentValue <= targetValue,
                _ => false
            };
        }
        catch (Exception) { return false; }
    }

    #endregion
}
