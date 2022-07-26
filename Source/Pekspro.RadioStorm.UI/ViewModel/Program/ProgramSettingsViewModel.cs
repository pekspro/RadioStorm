namespace Pekspro.RadioStorm.UI.ViewModel.Program;

public partial class ProgramSettingsViewModel : ObservableObject
{
    #region Start parameter

    record StartParameter(int ProgramId, string ProgramName);

    public static string CreateStartParameter(ProgramModel c) => CreateStartParameter
        (
            c.Id,
            c.Name
        );

    public static string CreateStartParameter(int programId, string programName) => StartParameterHelper.Serialize(new StartParameter(

            ProgramId: programId,
            ProgramName: programName
        ));

    #endregion

    #region Private properites

    private IEpisodesSortOrderManager EpisodesSortOrderManager { get; }

    private IDownloadSettings DownloadSettings { get; }
    
    private IGeneralDatabaseContextFactory GeneralDatabaseContextFactory { get; }

    private readonly int[] DownloadOptionsValues = new int[] { 0, 1, 2, 3, 5, 7, 10, 15, 20 };

    #endregion

    #region Constructor

    public ProgramSettingsViewModel()
        : this(null!, null!, null!)
    {
    }

    public ProgramSettingsViewModel(IEpisodesSortOrderManager episodesSortOrderManager, IDownloadSettings downloadSettings, IGeneralDatabaseContextFactory generalDatabaseContextFactory)
    {
        EpisodesSortOrderManager = episodesSortOrderManager;
        DownloadSettings = downloadSettings;
        GeneralDatabaseContextFactory = generalDatabaseContextFactory;
        var downloadOptionsItems = new List<StringIntComboItem>();

        for (int i = 0; i < DownloadOptionsValues.Length; i++)
        {
            int v = DownloadOptionsValues[i];
            if (v == 0)
            {
                downloadOptionsItems.Add(new StringIntComboItem(Strings.ProgramInfo_AutoDownload_QueueSize_None, 0));
            }
            else if (v == 1)
            {
                downloadOptionsItems.Add(new StringIntComboItem(Strings.ProgramInfo_AutoDownload_QueueSize_One, v));
            }
            else
            {
                downloadOptionsItems.Add(new StringIntComboItem(string.Format(Strings.ProgramInfo_AutoDownload_QueueSize_X, v), v));
            }
        }

        DownloadOptionsItems = downloadOptionsItems;


        var sortOrderStrings = new List<string>();

        sortOrderStrings.Add(Strings.ProgramInfo_Sorting_Order_NewestFirst);
        sortOrderStrings.Add(Strings.ProgramInfo_Sorting_Order_OldestFirst);

        SortOrderStrings = sortOrderStrings;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private int _ProgramId;

    [ObservableProperty]
    private string _ProgramName = string.Empty;

    public IReadOnlyList<string> SortOrderStrings { get; }

    public IReadOnlyList<StringIntComboItem> DownloadOptionsItems { get; }

    public bool IsBackgroundDownloadSupported => true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAutomaticDownloadActivated))]
    private StringIntComboItem _SelectedDownloadOptionItem = null!;

    partial void OnSelectedDownloadOptionItemChanged(StringIntComboItem value)
    {
        UpdateDownloadSettings();
    }

    public bool IsAutomaticDownloadActivated => SelectedDownloadOptionItem?.Value > 0;

    [ObservableProperty]
    private int _SortOrderPosition;

    partial void OnSortOrderPositionChanged(int value)
    {
        EpisodesSortOrderManager.SetFavorite(ProgramId, value != 0);
    }

    public bool SortOldestFirst
    {
        get
        {
            return SortOrderPosition != 0;
        }
        set
        {
            SortOrderPosition = SortOldestFirst ? 1 : 0;
        }
    }

    #endregion

    #region Methods

    public void OnNavigatedTo(object parameter)
    {
        StartParameter startParameter = StartParameterHelper.Deserialize<StartParameter>(parameter);

        ProgramId = startParameter.ProgramId;
        ProgramName = startParameter.ProgramName!;

        ReadSettings();
    }

    public void OnNavigatedFrom()
    {

    }

    private async void UpdateDownloadSettings()
    {
        DownloadSettings.UpdateSettings(ProgramId, SelectedDownloadOptionItem.Value);

        if (SelectedDownloadOptionItem.Value <= 0)
        {
            // Clear download history.
            await Task.Run(() =>
            {
                using var databaseContext = GeneralDatabaseContextFactory.Create();

                databaseContext.DownloadState.Where(a => a.ProgramId == ProgramId).BatchDelete();
            }).ConfigureAwait(false);
        }
    }

    public void ReadSettings()
    {
        var currentSetting = DownloadSettings.GetSettings(ProgramId);
        if (currentSetting is null)
        {
            _SelectedDownloadOptionItem = DownloadOptionsItems[0];
        }
        else
        {
            _SelectedDownloadOptionItem = DownloadOptionsItems.FirstOrDefault(a => a.Value == currentSetting.DownloadCount) ?? DownloadOptionsItems[0];
        }

        OnPropertyChanged(nameof(SelectedDownloadOptionItem));
        OnPropertyChanged(nameof(IsAutomaticDownloadActivated));

        _SortOrderPosition = EpisodesSortOrderManager.IsFavorite(ProgramId) ? 1 : 0;
        OnPropertyChanged(nameof(SortOldestFirst));
        OnPropertyChanged(nameof(SortOrderPosition));
    }

    #endregion
}
