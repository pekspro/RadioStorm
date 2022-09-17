namespace Pekspro.RadioStorm.UI.ViewModel.Song;

public sealed partial class SongsViewModel : DownloadViewModel, IDisposable
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private ISongModelFactory SongListItemModelFactory { get; }
    private IMainThreadTimer MainThreadTimer { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public SongsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        SongListItemModelFactory = null!;
        MainThreadTimer = null!;
        DownloadState = DownloadStates.Done;

        Items.Add(SongModel.CreateWithSampleData(0));
        Items.Add(SongModel.CreateWithSampleData(1));
        Items.Add(SongModel.CreateWithSampleData(2));
    }

    public SongsViewModel(
        IDataFetcher dataFetcher,
        ISongModelFactory songListItemModelFactory,
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        ILogger<SongsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        SongListItemModelFactory = songListItemModelFactory;
        MainThreadTimer = mainThreadTimerFactory.CreateTimer("Song timer");
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

    #region Public properties

    public bool IsChannel { get; set; }

    public int SourceId { get; set; }

    internal bool AutoRefresh => IsChannel;

    [ObservableProperty]
    private ObservableCollection<SongModel> _Items = new ObservableCollection<SongModel>();

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        MainThreadTimer.Stop();

        if (refreshSettings.FullRefresh)
        {
            DownloadState = DownloadStates.Downloading;
        }

        ObservableCollection<SongModel> items;

        if (IsChannel)
        {
            Logger.LogInformation($"Getting channel song list {SourceId} {refreshSettings}"); ;
            var data = await DataFetcher.GetChannelSongListAsync(SourceId, refreshSettings.AllowCache, cancellationToken);

            Logger.LogInformation($"Got channel song list {SourceId} {refreshSettings}. {data?.Count} items");

            items = new
                    (
                        from c in data
                        orderby c.PublishDate descending
                        select SongListItemModelFactory.Create(c, null!)
                    );
        }
        else
        {
            var episode = await DataFetcher.GetEpisodeAsync(SourceId, refreshSettings.AllowCache, cancellationToken);
            var data = await DataFetcher.GetEpisodeSongListAsync(SourceId, refreshSettings.AllowCache, cancellationToken);

            items = new
                    (
                        from c in data
                        orderby c.PublishDate descending
                        select SongListItemModelFactory.Create(c, episode)
                    );

        }

        if (Items is null || Items.Count != items.Count || Items.Count == 0 || !Items[0].Equals(items[0]))
        {
            Items = items;
            Logger.LogDebug($"Song list has been updated.");
        }
        else
        {
            Logger.LogDebug($"Song list hasn't changed.");
        }

        if (Items.Count > 0)
        {
            DownloadState = DownloadStates.Done;
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }

        if (AutoRefresh)
        {
            MainThreadTimer.Start();
        }
    }

    public void OnNavigatedTo(bool isChannel, int sourceId)
    {
        IsChannel = isChannel;
        SourceId = sourceId;

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
