namespace Pekspro.RadioStorm.UI.ViewModel.Player;

public partial class CurrentPlayingViewModel : DownloadViewModel, IDisposable
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IChannelModelFactory ChannelModelFactory { get; }
    private IAudioManager AudioManager { get; }
    private IChannelRefreshHelper ChannelRefreshHelper { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public CurrentPlayingViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        ChannelModelFactory = null!;
        AudioManager = null!;
        ChannelRefreshHelper = null!;
        EpisodeModelFactory = null!;

        DownloadState = DownloadStates.Done;
        ChannelData = ChannelModel.CreateWithSampleData();
        EpisodeData = EpisodeModel.CreateWithSampleData();
    }

    public CurrentPlayingViewModel(
        IDataFetcher dataFetcher,
        IChannelModelFactory channelModelFactory,
        IAudioManager audioManager,
        IChannelRefreshHelper channelStatusRefreshHelper,
        IEpisodeModelFactory episodeModelFactory,
        IMessenger messenger,
        IMainThreadRunner mainThreadRunner,
        ILogger<CurrentPlayingViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ChannelModelFactory = channelModelFactory;
        AudioManager = audioManager;
        ChannelRefreshHelper = channelStatusRefreshHelper;
        EpisodeModelFactory = episodeModelFactory;

        ChannelRefreshHelper.ViewModel = this;
        ChannelRefreshHelper.ChannelStatusTimer.SetupCallBack(() => QueueRefresh(new RefreshSettings(FullRefresh: false)));
        ChannelRefreshHelper.ChannelProgressTimer.SetupCallBack(() => ChannelRefreshHelper.RefreshChannelProgress(ChannelData));

        messenger.Register<PlaylistChanged>(this, (sender, message) =>
        {
            QueueRefresh(new RefreshSettings());
        });

        messenger.Register<CurrentItemChanged>(this, (sender, message) =>
        {
            QueueRefresh(new RefreshSettings());
        });
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private ChannelModel? _ChannelData = null;

    [ObservableProperty]
    private EpisodeModel? _EpisodeData;

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        try
        {
            ChannelRefreshHelper.Stop();

            var currentItem = AudioManager.CurrentItem;

            if (currentItem is null)
            {
                ChannelData = null;
                DownloadState = DownloadStates.NoData;
                return;
            }

            if (refreshSettings.FullRefresh)
            {
                DownloadState = DownloadStates.Downloading;
            }

            if (currentItem.IsLiveAudio)
            {
                EpisodeData = null;

                if (ChannelData is null || ChannelData.Id != currentItem.AudioId || refreshSettings.FullRefresh)
                {
                    var channelData = await DataFetcher.GetChannelAsync(currentItem.AudioId, refreshSettings.AllowCache, cancellationToken);

                    if (channelData is null)
                    {
                        DownloadState = DownloadStates.Error;
                        return;
                    }
                        
                    ChannelData = ChannelModelFactory.Create(channelData);
                }

                ChannelRefreshHelper.RefreshChannelProgress(ChannelData);

                await ChannelRefreshHelper.RefreshChannelStatusAsync
                (
                    DataFetcher,
                    ChannelData,
                    refreshSettings,
                    true,
                    cancellationToken
                );

                DownloadState = DownloadStates.Done;
            }
            else
            {
                ChannelData = null;
                ChannelRefreshHelper.Stop();

                var episodeData = await DataFetcher.GetEpisodeAsync(currentItem.AudioId, refreshSettings.AllowCache, cancellationToken);
                if (episodeData is not null)
                {
                    EpisodeData = EpisodeModelFactory.Create(episodeData);

                    DownloadState = DownloadStates.Done;
                }
                else
                {
                    DownloadState = DownloadStates.Error;
                }
            }
        }
        finally
        {
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        ChannelRefreshHelper.RefreshChannelProgress(ChannelData);

        base.OnNavigatedTo();
    }

    public override void OnNavigatedFrom()
    {
        ChannelRefreshHelper.Stop();

        base.OnNavigatedFrom();
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
                ChannelRefreshHelper.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
