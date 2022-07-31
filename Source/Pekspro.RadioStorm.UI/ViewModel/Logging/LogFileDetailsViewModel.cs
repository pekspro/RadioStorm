namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class LogFileDetailsViewModel : DownloadViewModel
{
    #region Start parameter

    record StartParameter(string LogFilePath);

    public static string CreateStartParameter(string logFileName) => 
        StartParameterHelper.Serialize(new StartParameter(logFileName));

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public LogFileDetailsViewModel()
        : base(null!, null!)
    {
        DownloadState = DownloadStates.Done;

        LogFilePath = "c:\\temp\\mylog.txt";
        LogFileContent = @"Hello world
Second line";
    }

    public LogFileDetailsViewModel(
        IMainThreadRunner mainThreadRunner,
        ILogger<LogFileDetailsViewModel> logger)
        : base(logger, mainThreadRunner)
    {
    }

    #endregion

    #region Properties

    public string Title => Path.GetFileName(LogFilePath);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private string _LogFilePath = string.Empty;

    [ObservableProperty]
    private string _LogFileContent = string.Empty;
    
    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        if (refreshSettings.FullRefresh || string.IsNullOrEmpty(LogFileContent))
        {
            DownloadState = DownloadStates.Downloading;

            try
            {
                // Open file and allow share with other processes
                using var stream = File.Open(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                LogFileContent = (await reader.ReadToEndAsync()).ReplaceLineEndings();
            }
            catch (Exception e)
            {
                LogFileContent = e.Message;
            }

            DownloadState = DownloadStates.Done;
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        StartParameter startParameter = StartParameterHelper.Deserialize<StartParameter>(parameter);

        if (startParameter.LogFilePath != _LogFilePath)
        {
            LogFilePath = startParameter.LogFilePath;
            LogFileContent = string.Empty;
        }

        base.OnNavigatedTo();
    }

    #endregion
}
