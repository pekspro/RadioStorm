namespace Pekspro.RadioStorm.GeneralDatabase.Models;

public class DownloadState
{
    public enum DownloadStatusEnum { NoDownloadAvailable, Downloaded }

    public int EpisodeId { get; set; }

    public int ProgramId { get; set; }

    public DownloadStatusEnum DownloadStatus { get; set; }
}
