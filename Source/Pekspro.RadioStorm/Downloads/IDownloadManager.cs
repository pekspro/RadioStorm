namespace Pekspro.RadioStorm.Downloads;

public interface IDownloadManager
{
    void DeleteDownload(int programId, int episodeId);
    Download? GetDownloadData(int programId, int episodeId);
    List<Download> GetDownloads();
    Task InitAsync();
    void StartDownload(int programId, int episodeId, string url, bool startedByUser);
    Task ShutDownAsync();
}