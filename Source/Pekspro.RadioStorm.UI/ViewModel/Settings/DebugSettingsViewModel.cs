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
    private List<string> _LogFilesFullPath = new List<string>();

    [ObservableProperty]
    private List<string> _LogFilesNameOnly = new List<string>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLogFileName))]
    [NotifyPropertyChangedFor(nameof(SelectedLogFilePath))]
    [NotifyPropertyChangedFor(nameof(CanReadSelectedLogFile))]
    [NotifyCanExecuteChangedFor(nameof(ReadSelectedLogFileCommand))]
    
    private int _SelectedLogFileIndex;
    
    public bool CanReadSelectedLogFile => LogFilesFullPath.Count > SelectedLogFileIndex && SelectedLogFileIndex >= 0;

    public string? SelectedLogFilePath =>
        LogFilesFullPath.Count > SelectedLogFileIndex && SelectedLogFileIndex >= 0 ? LogFilesFullPath[_SelectedLogFileIndex] : null;

    public string? SelectedLogFileName =>
        LogFilesNameOnly.Count > SelectedLogFileIndex && SelectedLogFileIndex >= 0 ? LogFilesNameOnly[_SelectedLogFileIndex] : null;

    [ObservableProperty]
    public string? _SelectedLogFileContent;

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
        RefreshLogFiles();
    }
    
    public async void RefreshLogFiles()
    {
        try
        {
            List<string> logFiles = await LogFileHelper.GetLogFileNamesAsync() ?? new List<string>();

            LogFilesNameOnly = logFiles.Select(a => Path.GetFileName(a) ?? a).ToList();
            LogFilesFullPath = logFiles;

            if (LogFilesNameOnly.Any())
            {
                SelectedLogFileIndex = LogFilesNameOnly.Count - 1;
                // OnPropertyChanged is needed when there are exactly 1 item.
                OnPropertyChanged(nameof(SelectedLogFileIndex));
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

    [RelayCommand(CanExecute = nameof(CanReadSelectedLogFile))]
    public async Task ReadSelectedLogFile()
    {
        if (string.IsNullOrWhiteSpace(SelectedLogFilePath))
        {
            return;
        }
        
        try
        {
            // Open file and allow share with other processes
            using var stream = File.Open(SelectedLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            SelectedLogFileContent = (await reader.ReadToEndAsync()).ReplaceLineEndings();
        }
        catch(Exception e)
        {
            SelectedLogFileContent = e.Message;
        }
    }

    #endregion
}
