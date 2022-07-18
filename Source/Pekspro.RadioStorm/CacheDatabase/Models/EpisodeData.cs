namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{EpisodeId} {Title}")]
public class EpisodeData
{
    public int EpisodeId { get; set; }

    public int? ProgramId { get; set; }
    public string? ProgramName { get; set; }

    public string? AudioStreamWithMusicUrl { get; set; }
    public string? AudioStreamWithoutMusicUrl { get; set; }
    public string? AudioDownloadUrl { get; set; }

    public int AudioStreamWithMusicDuration { get; set; }
    public int AudioStreamWithoutMusicDuration { get; set; }
    public int AudioDownloadDuration { get; set; }

    public DateTimeOffset PublishDate { get; set; }
    public DateTimeOffset? AudioStreamWithMusicExpireDate { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? EpisodeImage { get; set; }
    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
