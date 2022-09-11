namespace Pekspro.RadioStorm.Downloads;

public class Download
{
    public Download(string filename, int programId, int episodeId, bool startedByUser, string? programName, string? episodeTitle)
    {
        Filename = filename;
        ProgramId = programId;
        EpisodeId = episodeId;
        StartedByUser = startedByUser;
        ProgramName = programName;
        EpisodeTitle = episodeTitle;
    }

    public int ProgramId { get; }
    
    public int EpisodeId { get; }
    
    public bool StartedByUser { get; }
    
    public string? ProgramName { get; }
    
    public string? EpisodeTitle { get; }
    
    public string Filename { get; }

    public ulong BytesDownloaded { get; set; }
    
    public ulong BytesToDownload { get; set; }

    public DownloadDataStatus Status { get; set; }
    
    public DateTimeOffset DownloadTime { get; set; }
}
