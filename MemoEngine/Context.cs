using System;
using System.Collections.Generic;
using MemoEngine.Models;


namespace MemoEngine;

public class Context
{
    public IReadOnlyList<FormattableString> EventHistory { get; internal set; } = [];

    public EngineState Lifecycle   { get; internal set; } = EngineState.Idle;
    public uint        EnemyDataId { get; internal set; }

    public event Action<FightRecordPayload>? OnFightFinalized;

    internal void RaiseFightFinalized(FightRecordPayload payload) => OnFightFinalized?.Invoke(payload);
}
