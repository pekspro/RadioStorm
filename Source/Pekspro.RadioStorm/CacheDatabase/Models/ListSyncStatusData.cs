namespace Pekspro.RadioStorm.CacheDatabase.Models;

public sealed class ListSyncStatusData
{
    public enum ListType { Channels = 1, Programs = 2 }

    public ListType TypeId { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
