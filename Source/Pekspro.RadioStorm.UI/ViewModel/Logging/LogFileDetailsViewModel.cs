using Microsoft.Extensions.DependencyInjection;

namespace Pekspro.RadioStorm.UI.ViewModel.Logging;

public partial class LogFileDetailsViewModel : DownloadViewModel
{
    #region Start parameter

    class StartParameter
    {
        public string LogFilePath { get; set; } = null!;
    }

    [JsonSourceGenerationOptions()]
    [JsonSerializable(typeof(StartParameter))]
    partial class LogFileDetailsStartParameterJsonContext : JsonSerializerContext
    {
    }

    public static string CreateStartParameter(string logFileName) => 
        StartParameterHelper.Serialize(
            new StartParameter()
            {
                LogFilePath = logFileName,
            },
            LogFileDetailsStartParameterJsonContext.Default.StartParameter
        );

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
        ServiceProvider = null!;
    }

    public LogFileDetailsViewModel(
        IMainThreadRunner mainThreadRunner,
        ILogger<LogFileDetailsViewModel> logger,
        IServiceProvider serviceProvider)
        : base(logger, mainThreadRunner)
    {
        ServiceProvider = serviceProvider;
    }

    #endregion

    #region Properties

    private IServiceProvider ServiceProvider { get; }

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
                var fileLoggerProvider = ServiceProvider.GetService<FileLoggerProvider>();
                fileLoggerProvider?.Flush();

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
        StartParameter startParameter = StartParameterHelper.Deserialize<StartParameter>(parameter, LogFileDetailsStartParameterJsonContext.Default.StartParameter);

        if (startParameter.LogFilePath != _LogFilePath)
        {
            LogFilePath = startParameter.LogFilePath;
            LogFileContent = string.Empty;
        }

        base.OnNavigatedTo();
    }

    #endregion
}
