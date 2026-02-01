using System;


namespace MemoEngine.Models.Events;

internal interface ICombatantEvent : IEvent
{
    uint   DataId { get; }
    string Status { get; }
}

internal abstract class BaseCombatantEvent(DateTimeOffset timeStamp, uint dataId) : BaseEvent(timeStamp), ICombatantEvent
{
    public virtual uint   DataId => dataId;
    public virtual string Status => string.Empty;

    public override FormattableString Message => $"{base.Message} (data: {dataId})";
}

internal class CombatantSpawned(DateTimeOffset timeStamp, uint dataId) : BaseCombatantEvent(timeStamp, dataId)
{
    public override string Status => "SPAWN";
}

internal class CombatantDestroyed(DateTimeOffset timeStamp, uint dataId) : BaseCombatantEvent(timeStamp, dataId)
{
    public override string Status => "DESTROY";
}

internal class CombatantBecameTargetable(DateTimeOffset timeStamp, uint dataId) : BaseCombatantEvent(timeStamp, dataId)
{
    public override string Status => "TARGETABLE";
}

internal class CombatantBecameUntargetable(DateTimeOffset timeStamp, uint dataId) : BaseCombatantEvent(timeStamp, dataId)
{
    public override string Status => "UNTARGETABLE";
}

internal class CombatantHpUpdated(DateTimeOffset timeStamp, uint dataId, double currentHp, double maxHp) : BaseCombatantEvent(timeStamp, dataId)
{
    public double CurrentHp => currentHp;
    public double MaxHp     => maxHp;
}

public interface ICombatantSink
{
    void RaiseSpawned(DateTimeOffset timeStamp, uint dataId);

    void RaiseDestroyed(DateTimeOffset timeStamp, uint dataId);

    void RaiseBecameTargetable(DateTimeOffset timeStamp, uint dataId);

    void RaiseBecameUntargetable(DateTimeOffset timeStamp, uint dataId);

    void RaiseHpUpdated(DateTimeOffset timeStamp, uint dataId, double currentHp, double maxHp);
}

internal sealed class CombatantSink(Action<IEvent> postEvent) : ICombatantSink
{
    public void RaiseSpawned(DateTimeOffset timeStamp, uint dataId)
        => postEvent(new CombatantSpawned(timeStamp, dataId));

    public void RaiseDestroyed(DateTimeOffset timeStamp, uint dataId)
        => postEvent(new CombatantDestroyed(timeStamp, dataId));

    public void RaiseBecameTargetable(DateTimeOffset timeStamp, uint dataId)
        => postEvent(new CombatantBecameTargetable(timeStamp, dataId));

    public void RaiseBecameUntargetable(DateTimeOffset timeStamp, uint dataId)
        => postEvent(new CombatantBecameUntargetable(timeStamp, dataId));

    public void RaiseHpUpdated(DateTimeOffset timeStamp, uint dataId, double currentHp, double maxHp)
        => postEvent(new CombatantHpUpdated(timeStamp, dataId, currentHp, maxHp));
}
