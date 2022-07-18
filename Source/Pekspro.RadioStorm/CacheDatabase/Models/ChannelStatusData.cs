namespace Pekspro.RadioStorm.CacheDatabase.Models;

[DebuggerDisplay("{ChannelId} {CurrentProgram} ({LatestUpdateTime})")]
public class ChannelStatusData
{
    public ChannelStatusData()
    {

    }

    public int ChannelId { get; set; }

    public int? CurrentProgramId { get; set; }
    public string? CurrentProgram { get; set; }
    public string? CurrentProgramImage { get; set; }
    public string? CurrentProgramDescription { get; set; }
    public DateTimeOffset? CurrentStartTime { get; set; }
    public DateTimeOffset? CurrentEndTime { get; set; }

    public int? NextProgramId { get; set; }
    public string? NextProgram { get; set; }
    public string? NextProgramImage { get; set; }
    public string? NextProgramDescription { get; set; }
    public DateTimeOffset? NextStartTime { get; set; }
    public DateTimeOffset? NextEndTime { get; set; }
    public DateTimeOffset LatestUpdateTime { get; internal set; }
}
