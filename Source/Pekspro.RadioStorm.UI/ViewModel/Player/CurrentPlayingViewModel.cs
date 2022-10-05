namespace Pekspro.RadioStorm.UI.ViewModel.Player;

public sealed partial class CurrentPlayingViewModel : DownloadViewModel, IDisposable
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IChannelModelFactory ChannelModelFactory { get; }
    private IAudioManager AudioManager { get; }
    private IChannelRefreshHelper ChannelRefreshHelper { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IProgramModelFactory ProgramModelFactory { get; }

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
        PlayerViewModel = null!;
        ProgramModelFactory = null!;

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
        PlayerViewModel playerViewModel,
        ILogger<CurrentPlayingViewModel> logger,
        IProgramModelFactory programModelFactory)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ChannelModelFactory = channelModelFactory;
        AudioManager = audioManager;
        ChannelRefreshHelper = channelStatusRefreshHelper;
        EpisodeModelFactory = episodeModelFactory;
        PlayerViewModel = playerViewModel;
        ProgramModelFactory = programModelFactory;
        ChannelRefreshHelper.ViewModel = this;
        ChannelRefreshHelper.ChannelStatusTimer.SetupCallBack(() => QueueRefresh(new RefreshSettings(FullRefresh: false)));
        ChannelRefreshHelper.ChannelProgressTimer.SetupCallBack(() => ChannelRefreshHelper.RefreshChannelProgress(ChannelData));

        messenger.Register<PlaylistChanged>(this, (sender, message) =>
        {
            QueueRefresh(new RefreshSettings(FullRefresh: true));

            MainThreadRunner.RunInMainThread(() =>
            {
                OnPropertyChanged(nameof(HasPlayList));
                OnPropertyChanged(nameof(HasMorePlayListItems));
                OnPropertyChanged(nameof(PlayListItemIndex));
                OnPropertyChanged(nameof(PlayListItemCount));
            });
        });

        messenger.Register<CurrentItemChanged>(this, (sender, message) =>
        {
            QueueRefresh(new RefreshSettings(FullRefresh: true));

            MainThreadRunner.RunInMainThread(() =>
            {
                OnPropertyChanged(nameof(HasPlayList));
                OnPropertyChanged(nameof(HasMorePlayListItems));
                OnPropertyChanged(nameof(PlayListItemIndex));
                OnPropertyChanged(nameof(PlayListItemCount));
            });
        });
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private ChannelModel? _ChannelData = null;

    [ObservableProperty]
    private EpisodeModel? _EpisodeData;

    [ObservableProperty]
    private EpisodeModel? _NextEpisodeData;

    public bool HasPlayList => AudioManager.CurrentPlayList?.Items.Count > 0;

    public bool HasMorePlayListItems =>
        AudioManager.CurrentPlayList?.CanGoToNext == true;

    public int PlayListItemCount =>
        AudioManager.CurrentPlayList?.Items.Count ?? 1;

    public int PlayListItemIndex =>
        (AudioManager.CurrentPlayList?.CurrentPosition ?? 0) + 1;

    public PlayerViewModel PlayerViewModel { get; }
    
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
                EpisodeData = null;
                NextEpisodeData = null;
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
                NextEpisodeData = null;

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

                if (!HasMorePlayListItems)
                {
                    NextEpisodeData = null;
                }

                var episodeData = await DataFetcher.GetEpisodeAsync(currentItem.AudioId, refreshSettings.AllowCache, cancellationToken);
                if (episodeData is not null)
                {
                    EpisodeData = EpisodeModelFactory.Create(episodeData);

                    if (EpisodeData.ProgramId is not null)
                    {
                        var programData = await DataFetcher.GetProgramAsync(EpisodeData.ProgramId.Value, true, cancellationToken);

                        if (programData is not null)
                        {
                            EpisodeData.ProgramDetails = ProgramModelFactory.Create(programData);
                        }
                    }

                    var playList = AudioManager.CurrentPlayList;

                    if (playList is not null && playList.CanGoToNext)
                    {
                        var nextEpisodeData = await DataFetcher.GetEpisodeAsync
                            (
                                playList.Items[playList.CurrentPosition + 1].AudioId, 
                                refreshSettings.AllowCache, 
                                cancellationToken
                            );

                        if (nextEpisodeData is not null)
                        {
                            NextEpisodeData = EpisodeModelFactory.Create(nextEpisodeData);

                            if (NextEpisodeData.ProgramId is not null)
                            {
                                if (NextEpisodeData.ProgramId == EpisodeData.ProgramId)
                                {
                                    NextEpisodeData.ProgramDetails = EpisodeData.ProgramDetails;
                                }
                                else
                                {
                                    var programData = await DataFetcher.GetProgramAsync(NextEpisodeData.ProgramId.Value, true, cancellationToken);

                                    if (programData is not null)
                                    {
                                        EpisodeData.ProgramDetails = ProgramModelFactory.Create(programData);
                                    }
                                }
                            }
                        }
                    }

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
