using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace MemoEngine.Models;

public class FightRecordPayload
{
    [JsonProperty("start_time")]
    public DateTimeOffset StartTime { get; set; }

    [JsonProperty("duration")]
    public long Duration { get; set; } // nano seconds

    [JsonProperty("zone_id")]
    public uint ZoneId { get; set; }

    [JsonProperty("players")]
    public List<PlayerPayload> Players { get; set; } = [];

    [JsonProperty("clear")]
    public bool IsClear { get; set; }

    [JsonProperty("progress", Required = Required.Always)]
    public FightProgressPayload Progress { get; set; } = null!;
}

public class PlayerPayload
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("server")]
    public string Server { get; set; } = string.Empty;

    [JsonProperty("job_id")]
    public uint JobId { get; set; }

    [JsonProperty("level")]
    public uint Level { get; set; }

    [JsonProperty("death_count")]
    public uint DeathCount { get; set; }

    public PlayerPayload Clone()
    {
        return new PlayerPayload
        {
            Name       = Name,
            Server     = Server,
            JobId      = JobId,
            Level      = Level,
            DeathCount = DeathCount
        };
    }
}

public class FightProgressPayload
{
    [JsonProperty("phase")]
    public uint PhaseId { get; set; }

    [JsonProperty("subphase")]
    public uint SubphaseId { get; set; }

    [JsonProperty("enemy_id")]
    public uint EnemyId { get; set; }

    [JsonProperty("enemy_hp")]
    public double EnemyHp { get; set; }
}
