namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ProgramId} {Status} ({IncrementallyUpdateCount}) {LatestUpdateTime}")]
public sealed class EpisodeListSyncStatusData
{
    public enum SyncStatus { FullySynchronized, IncrementallyUpdated, HasSomeData }

    public int ProgramId { get; set; }
    public SyncStatus Status { get; set; }

    public int IncrementallyUpdateCount { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }

    public DateTimeOffset LatestFullSynchronizingTime { get; internal set; }
}
