namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class DebugSettingsViewModel : ObservableObject
{
    #region Private properties

    private ILogFileHelper LogFileHelper { get; }

    private ILogger Logger { get; }

    #endregion

    #region Constructor

    public DebugSettingsViewModel()
    {
        Settings = null!;
        LogFileHelper = null!;
        Logger = null!;
    }

    public DebugSettingsViewModel(ILocalSettings localSettings, ILogFileHelper logFileHelper, ILogger<DebugSettingsViewModel> logger)
    {
        Settings = localSettings;
        LogFileHelper = logFileHelper;
        Logger = logger;
    }

    #endregion

    #region Public properties

    public ILocalSettings Settings { get; }

    public bool ShowDebugSettings
    {
        get
        {
            return Settings.ShowDebugSettings;
        }
        set
        {
            if (Settings.ShowDebugSettings != value)
            {
                Settings.ShowDebugSettings = value;
                OnPropertyChanged(nameof(ShowDebugSettings));
            }
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogFiles))]
    [NotifyPropertyChangedFor(nameof(CanRemoveLogFiles))]
    [NotifyPropertyChangedFor(nameof(CanZipLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLogFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZipLogFilesCommand))]
    private List<string> _LogFiles = new List<string>();

    public bool HasLogFiles => LogFiles.Any();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanZipLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(ZipLogFilesCommand))]
    [NotifyPropertyChangedFor(nameof(CanRemoveLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLogFilesCommand))]
    private bool _IsRemovingLogFiles;

    public bool CanRemoveLogFiles => !IsRemovingLogFiles && HasLogFiles && !IsZippingLogFiles;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanZipLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(ZipLogFilesCommand))]
    [NotifyPropertyChangedFor(nameof(CanRemoveLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLogFilesCommand))]
    private bool _IsZippingLogFiles;

    public bool CanZipLogFiles => !IsZippingLogFiles && HasLogFiles && !IsRemovingLogFiles;

    public Action<string>? OnZipFileCreated { get; set; }

    #endregion

    #region Methods

    public void OnNavigatedTo()
    {
        RefreshLogFiles();
    }
    
    public async void RefreshLogFiles()
    {
        try
        {
            var logFiles = await LogFileHelper.GetLogFileNamesAsync();
            LogFiles = logFiles;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing log files");
            LogFiles = new List<string>();
        }
    }

    [RelayCommand(CanExecute = nameof(CanZipLogFiles))]
    public async Task ZipLogFiles()
    {
        IsZippingLogFiles = true;

        try
        {
            string zipFileName = await LogFileHelper.ZipAllLogFilesAsync();
        
            OnZipFileCreated?.Invoke(zipFileName);
        }
        catch (Exception )
        {

        }

        IsZippingLogFiles = false;
    }
    
    [RelayCommand(CanExecute = nameof(CanRemoveLogFiles))]
    public async Task RemoveLogFiles()
    {
        IsRemovingLogFiles = true;

        // Write a warning, will trigger a flus.
        Logger.LogWarning("Starts removing log files.");

        try
        {
            await LogFileHelper.RemoveOldLogFilesAsync(TimeSpan.FromMinutes(2));
        }
        catch (Exception )
        {

        }

        Logger.LogWarning("Removing log files completed.");

        IsRemovingLogFiles = false;

        RefreshLogFiles();
    }

    #endregion
}
