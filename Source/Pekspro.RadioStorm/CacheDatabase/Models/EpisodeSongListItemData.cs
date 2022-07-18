namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{EpisodeId} {Title} ({PublishDate})")]
public class EpisodeSongListItemData : SongListItemData
{
    public int EpisodeSongListItemDataId { get; set; }

    public int EpisodeId { get; set; }
}
