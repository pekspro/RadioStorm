namespace Pekspro.RadioStorm.CacheDatabase.Models;

public class SongListItemData
{
    public string Title { get; set; } = null!;
    public string? Artist { get; set; }
    public string? AlbumName { get; set; }
    public string? Composer { get; set; }

    public DateTimeOffset PublishDate { get; set; }
    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
