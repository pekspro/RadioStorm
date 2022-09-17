namespace Pekspro.RadioStorm.UI.ViewModel.SchedulesEpisode;

public sealed partial class SchedulesEpisodesViewModel : DownloadViewModel, IDisposable
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private ISchedulesEpisodeFactory SchedulesEpisodeFactory { get; }
    private IDateTimeProvider DateTimeProvider { get; }
    private IMainThreadTimer MainThreadTimer { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public SchedulesEpisodesViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        SchedulesEpisodeFactory = null!;
        MainThreadTimer = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        Items.Add(SchedulesEpisodeModel.CreateWithSampleData(0));
        Items.Add(SchedulesEpisodeModel.CreateWithSampleData(1));
        Items.Add(SchedulesEpisodeModel.CreateWithSampleData(2));
    }

    public SchedulesEpisodesViewModel(
        IDataFetcher dataFetcher,
        ISchedulesEpisodeFactory songListItemModelFactory,
        IMainThreadTimerFactory mainThreadTimerFactory,
        IDateTimeProvider dateTimeProvider,
        IMainThreadRunner mainThreadRunner,
        ILogger<SongsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        SchedulesEpisodeFactory = songListItemModelFactory;
        DateTimeProvider = dateTimeProvider;
        MainThreadTimer = mainThreadTimerFactory.CreateTimer("Scheduled episodes timer");
        MainThreadTimer.Interval = 2 * 60 * 1000;

        MainThreadTimer.SetupCallBack(() =>
        {
            if (IsActive)
            {
                Logger.LogInformation("Refreshing song list");
                QueueRefresh(new RefreshSettings(FullRefresh: false));
            }
        });
    }

    #endregion

    #region Properties

    public int ChannelId { get; set; }

    [ObservableProperty]
    public ObservableCollection<SchedulesEpisodeModel> _Items = new ObservableCollection<SchedulesEpisodeModel>();

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        MainThreadTimer.Stop();

        if (refreshSettings.FullRefresh)
        {
            DownloadState = DownloadStates.Downloading;
        }

        Items = new ObservableCollection<SchedulesEpisodeModel>();

        DateOnly swedishCurrentDate = DateOnly.FromDateTime(DateTimeProvider.SwedishNow);

        for (int i = 0; i < 10; i++)
        {
            var date = swedishCurrentDate.AddDays(i);
            Logger.LogInformation($"Getting scheduled episodes for channel {ChannelId} for date {date:yyyy-MM-dd} {refreshSettings}");
            var data = await DataFetcher.GetScheduledEpisodeListAsync(ChannelId, date, refreshSettings.AllowCache, cancellationToken);

            Logger.LogInformation($"Got scheduled episodes for channel {ChannelId} for date {date:yyyy-MM-dd} {refreshSettings}. {data!.Count} items");

            foreach (var item in data.OrderBy(a => a.Date))
            {
                var model = SchedulesEpisodeFactory.Create(item);

                Items.Add(model);
            }
        }

        if (Items.Count > 0)
        {
            DownloadState = DownloadStates.Done;
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }

        MainThreadTimer.Start();
    }

    public void OnNavigatedTo(int channelId)
    {
        ChannelId = channelId;

        MainThreadTimer.Start();

        base.OnNavigatedTo();

    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        MainThreadTimer.Stop();
    }

    #endregion

    #region Dispose

    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                MainThreadTimer.Dispose();
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
