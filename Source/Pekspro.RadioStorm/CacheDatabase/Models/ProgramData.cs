namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ProgramId} {Name}")]
public sealed class ProgramData
{
    public int ProgramId { get; set; }
    public string Name { get; set; } = null!;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Description { get; set; } = null!;
    public string? ProgramImageHighResolution { get; set; }
    public string? ProgramImageLowResolution { get; set; }
    public bool Archived { get; set; }
    public bool HasOnDemand { get; set; }
    public bool HasPod { get; set; }
    public string? BroadcastInfo { get; set; }
    public string? ProgramUri { get; set; }
    public string? FacebookPageUri { get; set; }
    public string? TwitterPageUri { get; set; }
    public int? ChannelId { get; set; }
    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
