namespace Pekspro.RadioStorm.CacheDatabase;

internal class CachePrefetcher : ICachePrefetcher
{
    #region Private properties

    private IProgramFavoriteList ProgramFavoriteList { get; }

    private IDownloadSettings DownloadSettings { get; }

    private IDataFetcher DataFetcher { get; }

    private IAutoDownloadManager AutoDownloadManager { get; }
    
    private ILogger Logger { get; }

    #endregion

    #region Constructor

    public CachePrefetcher
        (
            IProgramFavoriteList programFavoriteList, 
            IDownloadSettings downloadSettings, 
            IDataFetcher dataFetcher,
            IAutoDownloadManager autoDownloadManager,
            ILogger<CachePrefetcher> logger
        )
    {
        ProgramFavoriteList = programFavoriteList;
        DownloadSettings = downloadSettings;
        DataFetcher = dataFetcher;
        AutoDownloadManager = autoDownloadManager;
        Logger = logger;
    }

    #endregion

    #region Methods

    public async Task PrefetchAsync(CancellationToken cancellationToken)
    {
        var favoriteProgramIds = ProgramFavoriteList.Items.Where(a => a.Value.IsActive).Select(a => a.Key).ToList();
        var downloadProgramIds = DownloadSettings.GetProgramsWithDownloadSetting().Where(a => a.DownloadCount > 0).Select(a => a.ProgramId).ToList();

        var allIds = favoriteProgramIds.Union(downloadProgramIds).Distinct().ToList();

        Logger.LogInformation($"Starts prefetching {allIds.Count} programs.");

        {
            Random r = new Random();
            int randomValue = r.Next(30);

            if (randomValue < 3)
            {
                Logger.LogInformation("Randomness says we should update program list if it's out of date.");
                await DataFetcher.GetProgramsAsync(true, cancellationToken);
            }
            else if (randomValue == 10)
            {
                Logger.LogInformation("Randomness says we should update channel list if it's out of date.");
                await DataFetcher.GetChannelsAsync(true, cancellationToken);
            }
        }

        foreach (int programId in allIds)
        {
            var result = await DataFetcher.GetEpisodesAsync(programId, false, false, cancellationToken);

            //Make full update if just part of data is downloaded, or time for a refresh.
            if (result.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.GotSomePart)
            {
                Logger.LogInformation($"Updating program {programId} again. Has not all episodes.");
                result = await DataFetcher.GetEpisodesAsync(programId, true);
            }
            else if (result.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.IncrementalUpdate &&
                    CacheTime.IsTimeOut(result.LatestFullSyncronizingTime, CacheTime.EpisodeListFullSynchronzingIntervall)
                    )
            {
                Logger.LogInformation($"Updating program {programId} again. Time to refresh full episode list.");
                result = await DataFetcher.GetEpisodesAsync(programId, true);
            }

            if (result.Result is not RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.Error && result.Episodes is not null)
            {
                await AutoDownloadManager.StartDownloadAsync(result.Episodes.ToList(), cancellationToken);
            }
        }

        Logger.LogInformation($"Finished prefetching {allIds.Count} programs.");
    }

    #endregion
}
