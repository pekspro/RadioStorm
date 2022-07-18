namespace Pekspro.RadioStorm.Downloads;

internal interface IDownloadFetcher
{
    void StartDownload(Download downloadData, string url);
    void StopDownload(Download downloadData);
}