namespace Pekspro.RadioStorm.UI.ViewModel.Channel;

public partial class ChannelDetailsViewModel : DownloadViewModel, IDisposable
{
    #region Start parameter

    record StartParameter(int ChannelId, string? ChannelName, string? Color, string? ChannelImageUri);

    public static string? CreateStartParameter(ChannelModel c) => CreateStartParameter
        (
            c.Id,
            c.Title,
            c.ChannelColor,
            c.ChannelImage?.HighResolution
        );

    public static string? CreateStartParameter(SearchItem c) => CreateStartParameter
        (
            c.Id,
            c.Title,
            c.TitleColor,
            c.ImageLink?.HighResolution
        );

    public static string CreateStartParameter(int channelId, string? channelName, string? color, string? channelImageUri) => StartParameterHelper.Serialize(new StartParameter(

            ChannelId: channelId,
            ChannelName: channelName,
            Color: color,
            ChannelImageUri: channelImageUri
        ));

    #endregion


    //public static bool AutoPlayNextNavigation = false;

    //public bool AutoPlay = false;

    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IChannelModelFactory ChannelModelFactory { get; }
    private IAudioManager AudioManager { get; }
    private IChannelRefreshHelper ChannelRefreshHelper { get; }

    private string _Title = string.Empty;

    private string _ChannelColor = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public ChannelDetailsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        ChannelModelFactory = null!;
        AudioManager = null!;
        ChannelRefreshHelper = null!;
        SongsViewModel = new SongsViewModel();
        SchedulesEpisodesViewModel = new SchedulesEpisodesViewModel();
        DownloadState = DownloadStates.Done;

        _ChannelData = ChannelModel.CreateWithSampleData();
    }

    public ChannelDetailsViewModel(
        IDataFetcher dataFetcher,
        IChannelModelFactory channelModelFactory,
        IAudioManager audioManager,
        IChannelRefreshHelper channelStatusRefreshHelper,
        SongsViewModel songsViewModel,
        SchedulesEpisodesViewModel schedulesEpisodesViewModel,
        IMainThreadRunner mainThreadRunner,
        ILogger<ChannelDetailsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ChannelModelFactory = channelModelFactory;
        AudioManager = audioManager;
        ChannelRefreshHelper = channelStatusRefreshHelper;
        SongsViewModel = songsViewModel;
        SchedulesEpisodesViewModel = schedulesEpisodesViewModel;

        ChannelRefreshHelper.ViewModel = this;
        ChannelRefreshHelper.ChannelStatusTimer.SetupCallBack(() =>
        {
            QueueRefresh(new RefreshSettings(FullRefresh: false));
        });
        ChannelRefreshHelper.ChannelProgressTimer.SetupCallBack(() => ChannelRefreshHelper.RefreshChannelProgress(ChannelData));
    }

    #endregion

    #region Properties

    public int ChannelId { get; set; }

    public bool IsPlayingThis => AudioManager is not null && AudioManager.IsItemIdActive(true, ChannelId);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(ChannelColor))]
    private ChannelModel? _ChannelData = null;

    public string Title => ChannelData?.Title ?? _Title;

    public string ChannelColor => ChannelData?.ChannelColor ?? _ChannelColor;

    public SongsViewModel SongsViewModel { get; }

    public SchedulesEpisodesViewModel SchedulesEpisodesViewModel { get; }

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        try
        {
            ChannelRefreshHelper.ChannelStatusTimer.Stop();

            if (refreshSettings.FullRefresh || ChannelData is null)
            {
                DownloadState = DownloadStates.Downloading;
            }

            if (ChannelData is null || refreshSettings.FullRefresh)
            {
                var channelData = await DataFetcher.GetChannelAsync(ChannelId, refreshSettings.AllowCache, cancellationToken);

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
                new FavoriteBaseModel[] { ChannelData },
                refreshSettings.AllowCache,
                cancellationToken
            );

            DownloadState = DownloadStates.Done;
        }
        finally
        {
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        //AutoPlay = AutoPlayNextNavigation;
        //AutoPlayNextNavigation = false;

        StartParameter startParameter = StartParameterHelper.Deserialize<StartParameter>(parameter);

        if (startParameter.ChannelId != ChannelId)
        {
            ChannelId = startParameter.ChannelId;
            _Title = startParameter.ChannelName!;
            _ChannelColor = startParameter.Color!;
            ChannelData = null;

            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ChannelColor));
        }

        //Episodes.ChannelId = ChannelId;
        //await Episodes.OnNavigatedToAsync(parameter, mode, state);
        //await SongList.OnNavigatedToAsync(parameter, mode, state);

        //SongList.ChannelId = ChannelId;
        //SongList.AutoRefresh = true;
        //SongList.UpdateItems(true);

        ChannelRefreshHelper.RefreshChannelProgress(ChannelData);

        SongsViewModel.OnNavigatedTo(true, ChannelId);
        SchedulesEpisodesViewModel.OnNavigatedTo(ChannelId);

        base.OnNavigatedTo();
    }

    public override void OnNavigatedFrom()
    {
        ChannelRefreshHelper.Stop();

        SongsViewModel.OnNavigatedFrom();
        SchedulesEpisodesViewModel.OnNavigatedFrom();

        base.OnNavigatedFrom();
    }

    #endregion



    //      private void RefreshChannelData()
    //{
    //	// UpdateChannelInfo(false);
    //	_ = QueueRefreshAsync(new RefreshSettings());
    //}



    //public override async Task OnNavigatingFromAsync(NavigatingEventArgs e)
    //{
    //	//await base.OnNavigatingFromAsync(e);

    //	//await Episodes.OnNavigatingFromAsync(e);
    //	//await SongList.OnNavigatingFromAsync(e);

    //	//Timer.Stop();

    //	//RefreshChannelPositionTimer.Stop();
    //}

    //public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
    //{
    //	await Episodes.OnNavigatedFromAsync(state, suspending);
    //	await SongList.OnNavigatedFromAsync(state, suspending);

    //	await base.OnNavigatedFromAsync(state, suspending);
    //}



    //#region Episodes property

    //private ScheduledEpisodes.SchedulesEpisodesViewModel _Episodes;

    //public ScheduledEpisodes.SchedulesEpisodesViewModel Episodes
    //{
    //	get
    //	{
    //		if (_Episodes is null)
    //			_Episodes = new ScheduledEpisodes.SchedulesEpisodesViewModel();

    //		return _Episodes;
    //	}
    //	set
    //	{
    //		Set(ref _Episodes, value);
    //	}
    //}

    //#endregion

    //#region SongList property

    //private SongListViewModel _SongList = new SongListViewModel();

    //public SongListViewModel SongList
    //{
    //	get
    //	{
    //		return _SongList;
    //	}
    //	private set
    //	{
    //		Set(ref _SongList, value);
    //	}
    //}

    //#endregion

    #region Dispose

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
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
