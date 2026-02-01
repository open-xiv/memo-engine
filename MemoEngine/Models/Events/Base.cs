using System;


namespace MemoEngine.Models.Events;

internal interface IEvent
{
    DateTimeOffset    TimeStamp { get; }
    FormattableString Message   { get; }
}

internal abstract class BaseEvent(DateTimeOffset timeStamp) : IEvent
{
    public virtual DateTimeOffset    TimeStamp => timeStamp;
    public virtual FormattableString Message   => $"[{TimeStamp.ToUniversalTime().ToString("O")}:{GetType().Name}]";
}

public interface IEventSink
{
    IGeneralSink   General   { get; }
    IActionSink    Action    { get; }
    ICombatantSink Combatant { get; }
    IStatusSink    Status    { get; }
}

internal sealed class EventSink : IEventSink
{
    public IGeneralSink   General   { get; } = new GeneralSink();
    public IActionSink    Action    { get; } = new ActionSink();
    public ICombatantSink Combatant { get; } = new CombatantSink();
    public IStatusSink    Status    { get; } = new StatusSink();
}
