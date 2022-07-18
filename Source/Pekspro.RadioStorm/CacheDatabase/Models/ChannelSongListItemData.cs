namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} {Title} ({PublishDate})")]
public class ChannelSongListItemData : SongListItemData
{
    public int ChannelSongListItemDataId { get; set; }

    public int ChannelId { get; set; }
}
