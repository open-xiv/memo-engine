using System;
using System.Collections.Concurrent;
using MemoEngine.Models.Events;


namespace MemoEngine.Engine;

internal class EventRecorder(Context context, int maxEventHistory)
{
    private readonly ConcurrentQueue<FormattableString> eventHistory = [];

    public void Record(IEvent e)
    {
        eventHistory.Enqueue(e.Message);
        while (eventHistory.Count > maxEventHistory)
            eventHistory.TryDequeue(out _);

        context.EventHistory = eventHistory.ToArray();
    }
}
