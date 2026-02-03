using System;
using System.Collections.Generic;
using MemoEngine.Engine;


namespace MemoEngine.Models.Events;

internal class TerritoryChanged(DateTimeOffset timeStamp, ushort zoneId) : BaseEvent(timeStamp)
{
    public ushort ZoneId { get; } = zoneId;
}

internal class DutyCompleted(DateTimeOffset timeStamp) : BaseEvent(timeStamp);
internal class DutyWiped(DateTimeOffset     timeStamp) : BaseEvent(timeStamp);

internal class CombatOptIn(DateTimeOffset timeStamp, IReadOnlyDictionary<uint, PlayerPayload>? partyPlayers) : BaseEvent(timeStamp)
{
    public IReadOnlyDictionary<uint, PlayerPayload>? PartyPlayers { get; } = partyPlayers;
}

internal class CombatOptOut(DateTimeOffset timeStamp) : BaseEvent(timeStamp);

internal class PlayerDied(DateTimeOffset timeStamp, uint entityId) : BaseEvent(timeStamp)
{
    public uint EntityId { get; } = entityId;

    public override FormattableString Message => $"{base.Message} (entity: {EntityId})";
}

internal class PartyChanged(DateTimeOffset timeStamp, IReadOnlyList<uint> entityIds) : BaseEvent(timeStamp)
{
    public IReadOnlyList<uint> EntityIds { get; } = entityIds;
}

public interface IGeneralSink
{
    void RaiseTerritoryChanged(DateTimeOffset timeStamp, ushort zoneId);

    void RaiseDutyCompleted(DateTimeOffset timeStamp);

    void RaiseDutyWiped(DateTimeOffset timeStamp);

    void RaiseCombatOptIn(DateTimeOffset timeStamp, IReadOnlyDictionary<uint, PlayerPayload>? partyPlayers);

    void RaiseCombatOptOut(DateTimeOffset timeStamp);

    void RaisePlayerDied(DateTimeOffset timeStamp, uint entityId);

    void RaisePartyChanged(DateTimeOffset timeStamp, IReadOnlyList<uint> entityIds);
}

internal sealed class GeneralSink : IGeneralSink
{
    public void RaiseTerritoryChanged(DateTimeOffset timeStamp, ushort zoneId)
        => RuleEngine.PostEvent(new TerritoryChanged(timeStamp, zoneId));

    public void RaiseDutyCompleted(DateTimeOffset timeStamp)
        => RuleEngine.PostEvent(new DutyCompleted(timeStamp));

    public void RaiseDutyWiped(DateTimeOffset timeStamp)
        => RuleEngine.PostEvent(new DutyWiped(timeStamp));

    public void RaiseCombatOptIn(DateTimeOffset timeStamp, IReadOnlyDictionary<uint, PlayerPayload>? partyPlayers)
        => RuleEngine.PostEvent(new CombatOptIn(timeStamp, partyPlayers));

    public void RaiseCombatOptOut(DateTimeOffset timeStamp)
        => RuleEngine.PostEvent(new CombatOptOut(timeStamp));

    public void RaisePlayerDied(DateTimeOffset timeStamp, uint entityId)
        => RuleEngine.PostEvent(new PlayerDied(timeStamp, entityId));

    public void RaisePartyChanged(DateTimeOffset timeStamp, IReadOnlyList<uint> entityIds)
        => RuleEngine.PostEvent(new PartyChanged(timeStamp, entityIds));
}
