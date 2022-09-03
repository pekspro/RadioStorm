using System.Text.RegularExpressions;

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
    private List<LogLine> _LogLines = new List<LogLine>();
    
    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        if (refreshSettings.FullRefresh || LogLines.Count <= 0)
        {
            DownloadState = DownloadStates.Downloading;

            try
            {
                // Open file and allow share with other processes
                using var stream = File.Open(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);

                List<LogLine> logLines = new List<LogLine>();

                // Read one line at the time
                while (!reader.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    string? line = await reader.ReadLineAsync();

                    if (line != null)
                    {
                        //LogFileContent += line.ReplaceLineEndings();
                        // Split line by tabs, max 4 parts
                        string[] parts = line.Split('\t', 4);

                        // If line contains 4 parts, then it is a log line
                        if (parts.Length == 4)
                        {
                            // Check first part match the patten 2022-09-02 12:34:56
                            if (Regex.IsMatch(parts[0], @"\d\d\d\d-\d\d-\d\d \d\d:\d\d:\d\d"))
                            {
                                logLines.Add(new LogLine(parts[0], parts[1], parts[2], parts[3]));
                            }
                        }
                    }
                }

                LogLines = logLines;
            }
            catch (Exception )
            {
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
        }

        base.OnNavigatedTo();
    }

    #endregion
}
