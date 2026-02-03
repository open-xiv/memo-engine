using MemoEngine.Models.Events;


namespace MemoEngine;

public static class Event
{
    public static IGeneralSink   General   => EventSink.General;
    public static IActionSink    Action    => EventSink.Action;
    public static ICombatantSink Combatant => EventSink.Combatant;
    public static IStatusSink    Status    => EventSink.Status;
}
