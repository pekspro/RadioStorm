namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} ({LatestUpdateTime})")]
public class ChannelSongListSyncStatusData
{
    public int ChannelId { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
