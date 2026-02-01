using System;
using MemoEngine.Engine;


namespace MemoEngine.Models.Events;

internal interface IStatusEvent : IEvent
{
    uint   EntityId { get; }
    uint   StatusId { get; }
    string Status   { get; }
}

internal abstract class BaseStatusEvent(DateTimeOffset timeStamp, uint entityId, uint statusId) : BaseEvent(timeStamp), IStatusEvent
{
    public virtual uint   EntityId => entityId;
    public virtual uint   StatusId => statusId;
    public virtual string Status   => string.Empty;

    public override FormattableString Message => $"{base.Message} (entity: {entityId}) <status: {statusId}>";
}

internal class StatusApplied(DateTimeOffset timeStamp, uint entityId, uint statusId) : BaseStatusEvent(timeStamp, entityId, statusId)
{
    public override string Status => "APPLIED";
}

internal class StatusRemoved(DateTimeOffset timeStamp, uint entityId, uint statusId) : BaseStatusEvent(timeStamp, entityId, statusId)
{
    public override string Status => "REMOVED";
}

public interface IStatusSink
{
    void RaiseChanged(DateTimeOffset timeStamp, uint entityId, uint statusId);

    void RaiseRemoved(DateTimeOffset timeStamp, uint entityId, uint statusId);
}

internal sealed class StatusSink : IStatusSink
{
    public void RaiseChanged(DateTimeOffset timeStamp, uint entityId, uint statusId)
        => RuleEngine.PostEvent(new StatusApplied(timeStamp, entityId, statusId));

    public void RaiseRemoved(DateTimeOffset timeStamp, uint entityId, uint statusId)
        => RuleEngine.PostEvent(new StatusRemoved(timeStamp, entityId, statusId));
}
