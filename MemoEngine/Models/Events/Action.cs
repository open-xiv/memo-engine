using System;


namespace MemoEngine.Models.Events;

internal interface IActionEvent : IEvent
{
    uint   DataId   { get; }
    uint   ActionId { get; }
    string Status   { get; }
}

internal abstract class BaseActionEvent(DateTimeOffset timeStamp, uint dataId, uint actionId) : BaseEvent(timeStamp), IActionEvent
{
    public virtual uint   DataId   => dataId;
    public virtual uint   ActionId => actionId;
    public virtual string Status   => string.Empty;

    public override FormattableString Message => $"{base.Message} (data: {dataId}) <action: {actionId}>";
}

internal class ActionStarted(DateTimeOffset timeStamp, uint dataId, uint actionId) : BaseActionEvent(timeStamp, dataId, actionId)
{
    public override string Status => "START";
}

internal class ActionCompleted(DateTimeOffset timeStamp, uint dataId, uint actionId) : BaseActionEvent(timeStamp, dataId, actionId)
{
    public override string Status => "COMPLETE";
}

public interface IActionSink
{
    void RaiseStarted(DateTimeOffset timeStamp, uint dataId, uint actionId);

    void RaiseCompleted(DateTimeOffset timeStamp, uint dataId, uint actionId);
}

internal sealed class ActionSink(Action<IEvent> postEvent) : IActionSink
{
    public void RaiseStarted(DateTimeOffset timeStamp, uint dataId, uint actionId)
        => postEvent(new ActionStarted(timeStamp, dataId, actionId));

    public void RaiseCompleted(DateTimeOffset timeStamp, uint dataId, uint actionId)
        => postEvent(new ActionCompleted(timeStamp, dataId, actionId));
}
