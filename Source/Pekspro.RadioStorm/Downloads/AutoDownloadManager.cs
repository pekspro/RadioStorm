namespace Pekspro.RadioStorm.Downloads;

internal sealed class AutoDownloadManager : IAutoDownloadManager
{
    private IDownloadManager DownloadManager { get; }
    public IGeneralDatabaseContextFactory GeneralDatabaseContextFactory { get; }
    private IEpisodesSortOrderManager SortOrderManager { get; }

    private IDownloadSettings DownloadSettings { get; }

    private IListenStateManager ListenStateManager { get; }

    private ILogger<AutoDownloadManager> Logger { get; }

    public AutoDownloadManager(IDownloadManager downloadManager, IGeneralDatabaseContextFactory generalDatabaseContextFactory, IEpisodesSortOrderManager sortOrderManager, ILogger<AutoDownloadManager> logger, IDownloadSettings downloadSettings, IListenStateManager listenStateManager)
    {
        DownloadManager = downloadManager;
        GeneralDatabaseContextFactory = generalDatabaseContextFactory;
        Logger = logger;
        SortOrderManager = sortOrderManager;
        DownloadSettings = downloadSettings;
        ListenStateManager = listenStateManager;
    }

    public async Task StartDownloadAsync(List<EpisodeData> episodes, CancellationToken cancellationToken)
    {
        if (episodes.Count > 0)
        {
            if (episodes[0].ProgramId is null)
            {
                return;
            }

            int programId = episodes[0].ProgramId!.Value;

            var downloadSettings = DownloadSettings.GetSettings(programId);

            if (downloadSettings is null || downloadSettings.DownloadCount <= 0)
            {
                return;
            }

            Logger.LogInformation($"Checking if any downloads should be done from program {programId}... {episodes.Count} episodes.");

            episodes = episodes.OrderByDescending(a => a.PublishDate).ToList();

            if (SortOrderManager.IsFavorite(programId) == true)
            {
                episodes.Reverse();
            }


            int queueCount = 0;

            using var databaseContext = GeneralDatabaseContextFactory.Create();

            foreach (var episode in episodes)
            {
                //if (downloadStartCount >= maxDownloadStartCount)
                //    break;

                if (DownloadManager.GetDownloads().Count >= 100)
                {
                    break;
                }

                if (queueCount >= downloadSettings.DownloadCount)
                {
                    Logger.LogInformation($"Enough existing podcasts for program {programId} ({queueCount})");
                    break;
                }

                if (ListenStateManager.IsFullyListen(episode.EpisodeId))
                {
                    //Log($"Episode is already played {episode.EpisodeId} ({episode.Title})");
                    continue;
                }

                var urlToDownload = episode.AudioDownloadUrl;

                if (urlToDownload is null)
                {
                    // if (latestKnownEpisodePublishDate is null || latestKnownEpisodePublishDate.Value < episode.PublishDate)
                    {
                        databaseContext.DownloadState.Add(new DownloadState()
                        {
                            EpisodeId = episode.EpisodeId,
                            ProgramId = programId,
                            DownloadStatus = DownloadState.DownloadStatusEnum.NoDownloadAvailable
                        }
                        );

                        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    Logger.LogInformation($"No download support for {episode.EpisodeId} {episode.Title} ({episode.ProgramName})");
                    continue;
                }

                var latestDownloadState = await databaseContext.DownloadState.FirstOrDefaultAsync(a => a.EpisodeId == episode.EpisodeId).ConfigureAwait(false);

                if (latestDownloadState is not null && latestDownloadState.DownloadStatus != DownloadState.DownloadStatusEnum.NoDownloadAvailable)
                {
                    queueCount++;
                    Logger.LogInformation($"Episode {episode.EpisodeId} {episode.Title} ({episode.ProgramName}) has been downloaded previously.");
                    continue;
                }


                var currentDownloadState = DownloadManager.GetDownloadData(programId, episode.EpisodeId);
                queueCount++;

                if (currentDownloadState is not null)
                {
                    //Log($"Already downloaded: {programName} {episode.PublishDate.ToLocalTime().ToString("yyyy-MM-dd")} {episode.Title}");

                    continue;
                }
                else
                {
                    Logger.LogInformation($"Will request to download: {episode.ProgramId} {episode.PublishDate.ToLocalTime().ToString("yyyy-MM-dd")} {episode.Title} ({episode.ProgramName}) {urlToDownload}");

                    DownloadManager.StartDownload(programId, episode.EpisodeId, urlToDownload, false, null, null);

                    if (latestDownloadState is not null)
                    {
                        latestDownloadState.DownloadStatus = DownloadState.DownloadStatusEnum.Downloaded;
                    }
                    else
                    {
                        databaseContext.DownloadState.Add(new DownloadState()
                        {
                            EpisodeId = episode.EpisodeId,
                            ProgramId = programId,
                            DownloadStatus = DownloadState.DownloadStatusEnum.Downloaded
                        }
                        );
                    }

                    await databaseContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
