namespace Pekspro.RadioStorm.Logging;

public class InMemoryLogger : ILogger
{
    public int MaxLinesCount { get; set; } = 1000;

    public int NumberOfLinesToKeep { get; set; } = 500;

    private readonly List<LogRecord> _LogRecords = new List<LogRecord>();

    public IEnumerable<LogRecord> LogRecords => _LogRecords.AsReadOnly();

    public IDateTimeProvider DateTimeProvider { get; }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public InMemoryLogger(IDateTimeProvider dateTimeProvider)
    {
        DateTimeProvider = dateTimeProvider;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        lock (_LogRecords)
        {
            _LogRecords.Add(new LogRecord(logLevel, exception, message, DateTimeProvider.OffsetNow));

            if (_LogRecords.Count >= MaxLinesCount)
            {
                _LogRecords.RemoveRange(0, _LogRecords.Count - NumberOfLinesToKeep);
            }
        }
    }
}
