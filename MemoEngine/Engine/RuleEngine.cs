using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MemoEngine.Api;
using MemoEngine.Models;
using MemoEngine.Models.Events;


namespace MemoEngine.Engine;

internal static class RuleEngine
{
    // event queue & history
    private static readonly ActionBlock<IEvent> EventQueue;
    private static readonly EventRecorder       EventHistory;

    // fight context
    private static FightContext? FightContext;

    static RuleEngine()
    {
        EventHistory = new EventRecorder(1000);
        EventQueue   = new ActionBlock<IEvent>(ProcessEventAsync, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
    }


    public static void PostEvent(IEvent e)
        => EventQueue.Post(e);

    private static async Task ProcessEventAsync(IEvent e)
    {
        // event logs
        EventHistory.Record(e);

        if (e is TerritoryChanged tc)
        {
            var duty = await ApiClient.FetchDuty(tc.ZoneId);
            FightContext = duty is not null ? new FightContext(duty) : null;

            if (FightContext is null)
                Context.Lifecycle = EngineState.Idle;
        }

        FightContext?.ProcessEvent(e);
    }
}
