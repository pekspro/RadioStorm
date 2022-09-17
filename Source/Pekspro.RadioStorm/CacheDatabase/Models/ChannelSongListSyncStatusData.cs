namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} ({LatestUpdateTime})")]
public sealed class ChannelSongListSyncStatusData
{
    public int ChannelId { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
