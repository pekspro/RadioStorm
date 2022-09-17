namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} {Date} ({LatestUpdateTime})")]
public sealed class ScheduledEpisodeListSyncStatusData
{
    public int ChannelId { get; set; }

    public DateTimeOffset Date { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
