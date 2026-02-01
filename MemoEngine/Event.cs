using MemoEngine.Models;
using MemoEngine.Models.Events;


namespace MemoEngine;

public static class Event
{
    private static readonly EventSink Sink = new();

    public static IGeneralSink   General   => Sink.General;
    public static IActionSink    Action    => Sink.Action;
    public static ICombatantSink Combatant => Sink.Combatant;
    public static IStatusSink    Status    => Sink.Status;


    internal static bool Match(IActionEvent e, Trigger t)
    {
        if (t.Type != "ACTION_EVENT")
            return false;

        var actionMatch = t.ActionId.HasValue && t.ActionId.Value == e.ActionId;
        var statusMatch = t.Status == e.Status;

        return actionMatch && statusMatch;
    }

    internal static bool Match(ICombatantEvent e, Trigger t)
    {
        if (t.Type != "COMBATANT_EVENT")
            return false;

        var combatantMatch = t.NpcId.HasValue && t.NpcId.Value == e.DataId;
        var statusMatch    = t.Status == e.Status;

        return combatantMatch && statusMatch;
    }

    internal static bool Match(IStatusEvent e, Trigger t)
    {
        if (t.Type != "STATUS_EVENT")
            return false;

        var idMatch     = t.StatusId.HasValue && t.StatusId.Value == e.StatusId;
        var statusMatch = t.Status == e.Status;

        return idMatch && statusMatch;
    }
}
