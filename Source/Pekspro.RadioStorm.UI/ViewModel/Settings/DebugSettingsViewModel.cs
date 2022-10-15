namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public sealed partial class DebugSettingsViewModel : ObservableObject
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
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLogFiles))]
    [NotifyPropertyChangedFor(nameof(CanRemoveLogFiles))]
    [NotifyPropertyChangedFor(nameof(CanZipLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(RemoveLogFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZipLogFilesCommand))]
    private IReadOnlyList<string> _LogFilesFullPath = new List<string>();

    [ObservableProperty]
    private IReadOnlyList<string> _LogFilesNameOnly = new List<string>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLogFileName))]
    [NotifyPropertyChangedFor(nameof(SelectedLogFilePath))]
    [NotifyPropertyChangedFor(nameof(CanReadSelectedLogFile))]
    
    private int _SelectedLogFileIndex;
    
    public bool CanReadSelectedLogFile => LogFilesFullPath.Count > SelectedLogFileIndex && SelectedLogFileIndex >= 0;

    public string? SelectedLogFilePath =>
        LogFilesFullPath.Count > SelectedLogFileIndex && SelectedLogFileIndex >= 0 ? LogFilesFullPath[_SelectedLogFileIndex] : null;

    public string? SelectedLogFileName =>
        LogFilesNameOnly.Count > SelectedLogFileIndex && SelectedLogFileIndex >= 0 ? LogFilesNameOnly[_SelectedLogFileIndex] : null;

    public bool HasLogFiles => LogFilesFullPath.Any();

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
        if (LogFilesNameOnly.Count == 0)
        {
            RefreshLogFiles();
        }
    }
    
    public async void RefreshLogFiles()
    {
        try
        {
            List<string> logFiles = await LogFileHelper.GetLogFileNamesAsync() ?? new List<string>();

            LogFilesFullPath = logFiles.OrderBy(a => a).ToList();
            LogFilesNameOnly = logFiles.OrderBy(a => a).Select(a => Path.GetFileName(a) ?? a).ToList();

            // TODO: Remove when fixed: https://github.com/dotnet/maui/issues/9239
            LogFilesNameOnly = new List<string>(LogFilesNameOnly);

            if (LogFilesNameOnly.Any())
            {
                // Make sure SelectedLogFileIndex get changed.
                SelectedLogFileIndex = -1;
                SelectedLogFileIndex = LogFilesNameOnly.Count - 1;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing log files");
            LogFilesFullPath = new List<string>();
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

        // Write a warning, will trigger a flush.
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
