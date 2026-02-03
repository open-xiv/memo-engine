using System;
using System.Collections.Generic;
using MemoEngine.Models;


namespace MemoEngine;

public static class Context
{
    public static IReadOnlyList<FormattableString> EventHistory { get; internal set; } = [];

    public static EngineState Lifecycle   { get; internal set; } = EngineState.Idle;
    public static uint        EnemyDataId { get; internal set; }

    public static event Action<FightRecordPayload>? OnFightFinalized;

    internal static void RaiseFightFinalized(FightRecordPayload payload) => OnFightFinalized?.Invoke(payload);
}
