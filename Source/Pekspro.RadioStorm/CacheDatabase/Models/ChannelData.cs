namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} {Title}")]
public class ChannelData
{
    public int ChannelId { get; set; }
    public string Title { get; set; } = null!;
    public string ChannelGroupName { get; set; } = null!;
    public string? ChannelImageLowResolution { get; set; }
    public string? ChannelImageHighResolution { get; set; }
    public string? ChannelColor { get; set; }
    public string? LiveAudioUrl { get; set; }
    public string? WebPageUri { get; set; }

    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
