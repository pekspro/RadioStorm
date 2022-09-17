namespace Pekspro.RadioStorm.UI.ViewModel.Program;

public sealed partial class ProgramDetailsViewModel : DownloadViewModel
{
    #region Start parameter

    sealed class StartParameter
    {
        public int ProgramId { get; set; }

        public string? ProgramName { get; set; }
        
        public bool IsAutoScrollSupported { get; set; }
    }


    [JsonSourceGenerationOptions()]
    [JsonSerializable(typeof(StartParameter))]
    sealed partial class ProgramDetailsStartParameterJsonContext : JsonSerializerContext
    {
    }

    public static string CreateStartParameter(ProgramModel c, bool isAutoScrollSupported = false) => CreateStartParameter
        (
            c.Id,
            c.Name,
            isAutoScrollSupported
        );

    public static string CreateStartParameter(SearchItem c) => CreateStartParameter
        (
            c.Id,
            c.Title,
            false
        );

    public static string CreateStartParameter(int programId, string? programName, bool isAutoScrollSupported = false) => StartParameterHelper.Serialize(
        new StartParameter()
        {
            ProgramId = programId,
            ProgramName = programName,
            IsAutoScrollSupported = isAutoScrollSupported
        },
        ProgramDetailsStartParameterJsonContext.Default.StartParameter
    );

    #endregion

    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IProgramModelFactory ProgramModelFactory { get; }
    
    private string _ProgramName = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public ProgramDetailsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        ProgramModelFactory = null!;
        EpisodesViewModel = null!;
        DownloadState = DownloadStates.Done;

        EpisodesViewModel = new EpisodesViewModel();

        _ProgramData = ProgramModel.CreateWithSampleData();
    }

    public ProgramDetailsViewModel(
        IDataFetcher dataFetcher,
        IUriLauncher uriLauncher,
        IProgramModelFactory programModelFactory,
        EpisodesViewModel episodesViewModel,
        IMainThreadRunner mainThreadRunner,
        ILogger<ProgramDetailsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ProgramModelFactory = programModelFactory;
        EpisodesViewModel = episodesViewModel;
    }

    #endregion

    #region Properties

    public EpisodesViewModel EpisodesViewModel { get; }

    private int ProgramId
    {
        get
        {
            return EpisodesViewModel.ProgramId;
        }
        set
        {
            EpisodesViewModel.ProgramId = value;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    private ProgramModel? _ProgramData = null;

    public string Name => ProgramData?.Name ?? _ProgramName;

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        if (refreshSettings.FullRefresh || ProgramData is null)
        {
            DownloadState = DownloadStates.Downloading;
        }

        var programData = await DataFetcher.GetProgramAsync(ProgramId, refreshSettings.AllowCache, cancellationToken);

        if (programData is not null)
        {
            ProgramData = ProgramModelFactory.Create(programData);

            DownloadState = DownloadStates.Done;

            await EpisodesViewModel.RefreshAsync(refreshSettings, cancellationToken);
        }
        else
        {
            DownloadState = DownloadStates.Error;
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        StartParameter startParameter = StartParameterHelper.Deserialize<StartParameter>(parameter, ProgramDetailsStartParameterJsonContext.Default.StartParameter);

        if (startParameter.ProgramId != ProgramId)
        {
            ProgramId = startParameter.ProgramId;
            _ProgramName = startParameter.ProgramName!;
            ProgramData = null;

            OnPropertyChanged(nameof(Name));
        }

        base.OnNavigatedTo();
        EpisodesViewModel.OnNavigatedTo(false);
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        EpisodesViewModel.OnNavigatedFrom();
    }

    #endregion
}
