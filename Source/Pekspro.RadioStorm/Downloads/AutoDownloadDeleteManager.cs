namespace Pekspro.RadioStorm.Downloads;

internal class AutoDownloadDeleteManager : IAutoDownloadDeleteManager
{
    private IDownloadManager DownloadManager { get; }

    private ILocalSettings Settings { get; }

    private IListenStateManager ListenStateManager { get; }

    private ILogger<AutoDownloadDeleteManager> Logger { get; }
    
    private IDateTimeProvider DateTimeProvider { get; }

    public AutoDownloadDeleteManager
        (
            IDownloadManager downloadManager, 
            ILogger<AutoDownloadDeleteManager> logger, 
            IDateTimeProvider dateTimeProvider,
            ILocalSettings settings, 
            IListenStateManager listenStateManager
        )
    {
        DownloadManager = downloadManager;
        Logger = logger;
        DateTimeProvider = dateTimeProvider;
        Settings = settings;
        ListenStateManager = listenStateManager;
    }

    public void DeleteObseleteDownloads()
    {
        if (Settings.AutoRemoveListenedDownloadedFilesDayDelay < 0)
        {
            Logger.LogInformation($"Auto deleting downloads is disabled.");
            return;
        }

        var downloads = DownloadManager.GetDownloads();
        var maxActiveTime = DateTimeProvider.OffsetNow.AddDays(-Settings.AutoRemoveListenedDownloadedFilesDayDelay);

        foreach (var downloadToDelete in downloads.Where
            (
                a => a.Status == DownloadDataStatus.Done &&
                     ListenStateManager.IsFullyListen(a.EpisodeId)
            ))
        {
            var listenState = ListenStateManager.Items[downloadToDelete.EpisodeId];

            DateTimeOffset latestListenState = TimestampHelper.ToDateTime(listenState.LastChangedTimestamp);

            if (latestListenState < maxActiveTime && downloadToDelete.DownloadTime < maxActiveTime)
            {
                Logger.LogInformation($"Deleting download {downloadToDelete.EpisodeId}");

                DownloadManager.DeleteDownload(downloadToDelete.ProgramId, downloadToDelete.EpisodeId);
            }
        }
    }
}
