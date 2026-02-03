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
    public static IGeneralSink   General   { get; } = new GeneralSink();
    public static IActionSink    Action    { get; } = new ActionSink();
    public static ICombatantSink Combatant { get; } = new CombatantSink();
    public static IStatusSink    Status    { get; } = new StatusSink();
    
    IGeneralSink   IEventSink.General   => General;
    IActionSink    IEventSink.Action    => Action;
    ICombatantSink IEventSink.Combatant => Combatant;
    IStatusSink    IEventSink.Status    => Status;
}
