namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{EpisodeId} ({LatestUpdateTime})")]
public sealed class EpisodeSongListSyncStatusData
{
    public int EpisodeId { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
