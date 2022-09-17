namespace Pekspro.RadioStorm.UI.ViewModel.Logging;

public sealed partial class LoggingViewModel : DownloadViewModel, IDisposable
{
    #region Private properties

    private IMainThreadTimer Timer;
    private InMemLoggerProvider MemoryLoggerProvider { get; }

    private LogRecord? LastLogRecord = null;

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public LoggingViewModel()
        : base(null!, null!)
    {
        Timer = null!;
        MemoryLoggerProvider = null!;
        DownloadState = DownloadStates.Done;

        int offset = 0;
        foreach (var logLevel in Enum.GetValues<LogLevel>())
        {
            LogRecords.Add(
                new LogRecord
                (
                    LogLevel: logLevel,
                    Exception: null,
                    Category: "The cateogory",
                    Message: $"Message with log level type {logLevel.ToString()}",
                    Timestamp: SampleData.SampleTime.AddSeconds(offset)
                ));

            offset += 7;
        }
    }

    public LoggingViewModel(
        InMemLoggerProvider memoryLoggerProvider,
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        ILogger<LoggingViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        Timer = mainThreadTimerFactory.CreateTimer("Logging refresh");

        Timer.SetupCallBack(() => QueueRefresh(new RefreshSettings(FullRefresh: false)));
        MemoryLoggerProvider = memoryLoggerProvider;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private ObservableCollection<LogRecord> _LogRecords = new ObservableCollection<LogRecord>();

    #endregion

    #region Methods
    
    internal override Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        Timer.Stop();

        if (!MemoryLoggerProvider.LogRecords.Any())
        {
            DownloadState = DownloadStates.NoData;
        }
        else
        {
            if (MemoryLoggerProvider.LogRecords.Last() != LastLogRecord)
            {
                int fromPos = 0;
                var records = MemoryLoggerProvider.LogRecords.ToList();

                if (LastLogRecord is not null)
                {
                    fromPos = Math.Max(0, records.IndexOf(LastLogRecord)) + 1;
                }

                for (int i = fromPos; i < records.Count; i++)
                {
                    LogRecords.Add(records[i]);
                }

                LastLogRecord = records.Last();

                DownloadState = DownloadStates.Done;
            }
        }

        Timer.Stop();
        Timer.Interval = 2000;
        Timer.Start();

        return Task.CompletedTask;
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        Timer.Stop();
    }

    #endregion

    #region Dispose

    private bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Timer.Dispose();

                Timer = null!;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
