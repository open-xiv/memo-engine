using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MemoEngine.Api;
using MemoEngine.Models;
using MemoEngine.Models.Events;


namespace MemoEngine.Engine;

internal class RuleEngine : IDisposable
{
    // event queue & history
    private readonly ActionBlock<IEvent> eventQueue;
    private readonly EventRecorder       eventHistory;

    // lib & fight context
    private readonly Context       context;
    private          FightContext? fightContext;

    // http client
    private readonly ApiClient apiClient;

    public RuleEngine(Context context)
    {
        this.context = context;
        apiClient    = new ApiClient();
        eventHistory = new EventRecorder(context, 1000);
        eventQueue   = new ActionBlock<IEvent>(ProcessEventAsync, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
    }


    public void PostEvent(IEvent e)
        => eventQueue.Post(e);

    private async Task ProcessEventAsync(IEvent e)
    {
        // event logs
        eventHistory.Record(e);

        if (e is TerritoryChanged tc)
        {
            var duty = await apiClient.FetchDuty(tc.ZoneId);
            fightContext = duty is not null ? new FightContext(context, duty) : null;

            if (fightContext is null)
                context.Lifecycle = EngineState.Idle;
        }

        fightContext?.ProcessEvent(e);
    }

    public void Dispose()
        => apiClient.Dispose();
}
