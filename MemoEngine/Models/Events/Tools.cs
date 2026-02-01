namespace MemoEngine.Models.Events;

internal static class EventTools
{
    public static bool Match(IActionEvent e, Trigger t)
    {
        if (t.Type != "ACTION_EVENT")
            return false;

        var actionMatch = t.ActionId.HasValue && t.ActionId.Value == e.ActionId;
        var statusMatch = t.Status == e.Status;

        return actionMatch && statusMatch;
    }

    public static bool Match(ICombatantEvent e, Trigger t)
    {
        if (t.Type != "COMBATANT_EVENT")
            return false;

        var combatantMatch = t.NpcId.HasValue && t.NpcId.Value == e.DataId;
        var statusMatch    = t.Status == e.Status;

        return combatantMatch && statusMatch;
    }

    public static bool Match(IStatusEvent e, Trigger t)
    {
        if (t.Type != "STATUS_EVENT")
            return false;

        var idMatch     = t.StatusId.HasValue && t.StatusId.Value == e.StatusId;
        var statusMatch = t.Status == e.Status;

        return idMatch && statusMatch;
    }
}
