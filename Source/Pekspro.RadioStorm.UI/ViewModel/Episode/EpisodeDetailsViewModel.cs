namespace Pekspro.RadioStorm.UI.ViewModel.Episode;

public sealed partial class EpisodeDetailsViewModel : DownloadViewModel
{
    #region Start parameter

    internal sealed class StartParameter
    {
        public int EpisodeId { get; set; }
        public string? Title { get; set; }
        public bool AllowNavigationToProgramInfoPage { get; set; }
    }

    [JsonSourceGenerationOptions()]
    [JsonSerializable(typeof(StartParameter))]
    internal sealed partial class EpisodeDetailsStartParameterJsonContext : JsonSerializerContext
    {
    }

    public static string CreateStartParameter(EpisodeModel c, bool allowNavigationToProgramInfoPage) => CreateStartParameter
        (
            c.Id,
            c.Title,
            allowNavigationToProgramInfoPage
        );

    public static string CreateStartParameter(SearchItem c) => CreateStartParameter
        (
            c.Id,
            c.Title,
            true
        );

    public static string CreateStartParameter(int episodeId, string title, bool allowNavigationToProgramInfoPage) => StartParameterHelper.Serialize(
        new StartParameter()
        {
            EpisodeId = episodeId,
            Title = title,
            AllowNavigationToProgramInfoPage = allowNavigationToProgramInfoPage
        },
        EpisodeDetailsStartParameterJsonContext.Default.StartParameter
    );

    #endregion

    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IUriLauncher UriLauncher { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }

    private string _Title = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public EpisodeDetailsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        UriLauncher = null!;
        EpisodeModelFactory = null!;
        DownloadState = DownloadStates.Done;
        AllowNavigationToProgramInfoPage = true;

        EpisodeData = EpisodeModel.CreateWithSampleData();
        PreviousEpisodeData = EpisodeModel.CreateWithSampleData(1);
        NextEpisodeData = EpisodeModel.CreateWithSampleData(2);
    }

    public EpisodeDetailsViewModel(
        IDataFetcher dataFetcher,
        IUriLauncher uriLauncher,
        IEpisodeModelFactory episodeModelFactory,
        IMainThreadRunner mainThreadRunner,
        ILogger<EpisodeDetailsViewModel> logger)
        : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        UriLauncher = uriLauncher;
        EpisodeModelFactory = episodeModelFactory;
    }

    #endregion

    #region Properties

    private int EpisodeId { get; set; } = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(NavigationToProgramPageIsPossible))]
    private EpisodeModel? _EpisodeData;

    public string Title => EpisodeData?.Title ?? _Title;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNextEpisodeData))]
    private EpisodeModel? _NextEpisodeData;

    public bool HasNextEpisodeData => NextEpisodeData is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreviousEpisodeData))]
    private EpisodeModel? _PreviousEpisodeData;

    public bool HasPreviousEpisodeData => PreviousEpisodeData is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NavigationToProgramPageIsPossible))]
    private bool _AllowNavigationToProgramInfoPage;

    public bool NavigationToProgramPageIsPossible => 
                AllowNavigationToProgramInfoPage && 
                EpisodeData?.ProgramId is not null &&
                EpisodeData?.ProgramId.Value != 0;

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        if (EpisodeId < 0)
        {
            DownloadState = DownloadStates.Error;
            return;
        }

        if (refreshSettings.FullRefresh || EpisodeData is null)
        {
            DownloadState = DownloadStates.Downloading;
        }

        var episodeData = await DataFetcher.GetEpisodeAsync(EpisodeId, refreshSettings.AllowCache, cancellationToken);
        if (episodeData is not null)
        {
            EpisodeData = EpisodeModelFactory.Create(episodeData);
            OnPropertyChanged(nameof(NavigationToProgramPageIsPossible));

            DownloadState = DownloadStates.Done;

            if (EpisodeData.PublishLength.PublishDate.HasValue && EpisodeData.ProgramId.HasValue)
            {
                var prevEpisodeData =
                    await DataFetcher.GetPreviousEpisodeAsync(EpisodeData.ProgramId.Value, EpisodeData.PublishLength.PublishDate.Value);

                if (prevEpisodeData is not null)
                {
                    PreviousEpisodeData = EpisodeModelFactory.Create(prevEpisodeData);
                }
                else
                {
                    PreviousEpisodeData = null;
                }

                var nextEpisodeData =
                    await DataFetcher.GetNextEpisodeAsync(EpisodeData.ProgramId.Value, EpisodeData.PublishLength.PublishDate.Value);

                if (nextEpisodeData is not null)
                {
                    NextEpisodeData = EpisodeModelFactory.Create(nextEpisodeData);
                }
                else
                {
                    NextEpisodeData = null;
                }
            }
        }
        else
        {
            DownloadState = DownloadStates.Error;
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        StartParameter startParameter = StartParameterHelper.Deserialize<StartParameter>(parameter, EpisodeDetailsStartParameterJsonContext.Default.StartParameter);

        if (startParameter.EpisodeId != EpisodeId)
        {
            EpisodeId = startParameter.EpisodeId;
            AllowNavigationToProgramInfoPage = startParameter.AllowNavigationToProgramInfoPage;
            EpisodeData = null;
            NextEpisodeData = null;
            PreviousEpisodeData = null;
            _Title = startParameter.Title ?? _Title;

            OnPropertyChanged(nameof(Title));
        }

        // SongsViewModel.OnNavigatedTo(false, EpisodeId);

        OnNavigatedTo();
    }

    public override void OnNavigatedFrom()
    {
        // SongsViewModel.OnNavigatedFrom();

        base.OnNavigatedFrom();
    }

    #endregion
}
