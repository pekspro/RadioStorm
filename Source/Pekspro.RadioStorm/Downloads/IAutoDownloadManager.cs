namespace Pekspro.RadioStorm.Downloads;

internal interface IAutoDownloadManager
{
    Task StartDownloadAsync(List<EpisodeData> episodes, CancellationToken cancellationToken);
}