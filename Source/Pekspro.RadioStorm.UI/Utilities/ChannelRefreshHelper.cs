namespace Pekspro.RadioStorm.UI.Utilities;

public class ChannelRefreshHelper : IChannelRefreshHelper
{
    #region private properties

    private static TimeSpan ChannelProgressUpdateIntervall = TimeSpan.FromSeconds(2);

    private IRefreshTimerHelper RefreshTimerHelper { get; }
    private IDateTimeProvider DateTimeProvider { get; }
    private ILogger Logger { get; }

    #endregion

    #region Constructor
    
    public ChannelRefreshHelper(
        IRefreshTimerHelper refreshTimerHelper,
        IMainThreadTimerFactory mainThreadTimerFactory,
        IDateTimeProvider dateTimeProvider,
        ILogger<ChannelRefreshHelper> logger)
    {
        RefreshTimerHelper = refreshTimerHelper;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;
        ChannelStatusTimer = mainThreadTimerFactory.CreateTimer("Status");

        ChannelProgressTimer = mainThreadTimerFactory.CreateTimer("Progress");
        ChannelProgressTimer.Interval = ChannelProgressUpdateIntervall.TotalMilliseconds;
    }

    #endregion

    #region Properties

    public IMainThreadTimer ChannelStatusTimer { get; }

    public IMainThreadTimer ChannelProgressTimer { get; }

    public DownloadViewModel? ViewModel { get; set; }

    #endregion

    #region Methods

    public void Stop()
    {
        ChannelStatusTimer.Stop();
        ChannelProgressTimer.Stop();
    }

    public Task RefreshChannelStatusAsync
    (
        IDataFetcher dataFetcher,
        FavoriteBaseModel? model,
        RefreshSettings refreshSettings,
        bool setupTimer,
        CancellationToken cancellationToken
    )
    {
        return RefreshChannelStatusAsync
        (
            dataFetcher,
            new[] { model },
            refreshSettings,
            setupTimer,
            cancellationToken
        );
    }
    
    public async Task RefreshChannelStatusAsync
        (
            IDataFetcher dataFetcher,
            IEnumerable<FavoriteBaseModel?>? models,
            RefreshSettings refreshSettings,
            bool setupTimer,
            CancellationToken cancellationToken
        )
    {
        ChannelStatusTimer.Stop();

        if (ViewModel is not null && ViewModel.IsActive == false)
        {
            Logger.LogWarning("View model is not active. Will not refresh channel status.");
            return;
        }

        if (models is not null)
        {
            var channels = models.Where(a => a is ChannelModel).Select(a => (ChannelModel)a!).ToList();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Logger.LogDebug($"Updating channel status start. {channels.Count} channels.");

            if (channels.Any())
            {
                IList<ChannelStatusData>? updatedStatus = null;

                if (channels.Count == 1)
                {
                    var channelStatus = await dataFetcher.GetChannelStatusAsync(channels[0].Id, refreshSettings.AllowCache, cancellationToken);

                    if (channelStatus is not null)
                    {
                        updatedStatus = new List<ChannelStatusData>()
                        {
                            channelStatus
                        };
                    }
                }
                else
                {
                    updatedStatus = await dataFetcher.GetChannelStatusesAsync(refreshSettings.AllowCache, cancellationToken);
                }

                if (updatedStatus is not null)
                {
                    foreach (var data in channels)
                    {
                        var playingData = updatedStatus.FirstOrDefault(f => f.ChannelId == data.Id);

                        if (playingData is not null)
                        {
                            data.Status = new ChannelStatusModel(playingData, DateTimeProvider, data.ChannelImage);
                        }
                    }
                }
            }
        
            Logger.LogDebug($"Updating channels completed in {stopwatch.ElapsedMilliseconds} ms.");
        }

        if (setupTimer)
        {
            SetupStatusRefreshTimer(models);
        }
    }

    public void SetupStatusRefreshTimer(IEnumerable<FavoriteBaseModel?>? models)
    {
        ChannelStatusTimer.Stop();

        if (ViewModel is not null && ViewModel.IsActive == false)
        {
            Logger.LogWarning("View model is not active. Will not setup refresh timer.");
            return;
        }

        DateTimeOffset? nextRefreshTime = null;

        if (models is not null)
        {
            var channels = models
                                .Where(a => a is ChannelModel)
                                .Select(a => (ChannelModel) a!);

            nextRefreshTime = channels
                                .Where(c => c.Status?.NextRefreshTime is not null)
                                .Min(c => c.Status?.NextRefreshTime);

            if (nextRefreshTime is null)
            {
                nextRefreshTime = DateTimeProvider.OffsetNow.AddHours(1);
            }
        }

        RefreshTimerHelper.SetupTimer(ChannelStatusTimer, nextRefreshTime);
    }

    public void RefreshChannelProgress(FavoriteBaseModel? models)
    {
        RefreshChannelProgress(new[] { models });
    }
    
    public void RefreshChannelProgress(IEnumerable<FavoriteBaseModel?>? models)
    {
        ChannelProgressTimer.Stop();

        if (ViewModel is not null && ViewModel.IsActive == false)
        {
            Logger.LogWarning("View model is not active. Will not refresh channel progress.");
            return;
        }

        if (models is not null)
        {
            foreach (var mod in models)
            {
                if (mod is ChannelModel channelModel)
                {
                    channelModel.Status?.NotifyCurrentProgressChanged();
                }
            }
        }

        ChannelProgressTimer.Interval = ChannelProgressUpdateIntervall.TotalMilliseconds;
        ChannelProgressTimer.Start();
    }

    #endregion

    #region Dispose

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ChannelStatusTimer.Dispose();
                ChannelProgressTimer.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
