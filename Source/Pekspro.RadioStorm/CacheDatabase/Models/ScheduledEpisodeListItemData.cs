namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} {Date} {Title}")]
public sealed class ScheduledEpisodeListItemData
{
    public int ScheduledEpisodeDataId { get; set; }
    public int ChannelId { get; set; }
    public DateTimeOffset Date { get; set; }

    public int EpisodeId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTimeOffset StartTimeUtc { get; set; }
    public DateTimeOffset EndTimeUtc { get; set; }
    public int ProgramId { get; set; }
    public string? ProgramName { get; set; }
}
