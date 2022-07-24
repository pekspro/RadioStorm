namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class DebugSettingsViewModel : ObservableObject
{
    #region Private properties

    private ILogFileHelper LogFileHelper { get; }

    #endregion

    #region Constructor

    public DebugSettingsViewModel()
    {
        Settings = null!;
        LogFileHelper = null!;
    }

    public DebugSettingsViewModel(ILocalSettings localSettings, ILogFileHelper logFileHelper)
    {
        Settings = localSettings;
        LogFileHelper = logFileHelper;
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
    [NotifyPropertyChangedFor(nameof(CanZipLogFiles))]
    [NotifyCanExecuteChangedFor(nameof(ZipLogFilesCommand))]
    private bool _IsZippingLogFiles;

    public bool CanZipLogFiles => !IsZippingLogFiles;

    public Action<string>? OnZipFileCreated { get; set; }

    #endregion

    #region Methods

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

    #endregion
}
