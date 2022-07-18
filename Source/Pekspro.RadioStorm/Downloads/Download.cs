﻿namespace Pekspro.RadioStorm.Downloads;

public class Download
{
    public Download(string filename, int programId, int episodeId)
    {
        Filename = filename;
        ProgramId = programId;
        EpisodeId = episodeId;
    }

    public int ProgramId { get; }
    
    public int EpisodeId { get; }

    public string Filename { get; }

    public ulong BytesDownloaded { get; set; }
    
    public ulong BytesToDownload { get; set; }

    public DownloadDataStatus Status { get; set; }
    
    public DateTimeOffset DownloadTime { get; set; }
}
