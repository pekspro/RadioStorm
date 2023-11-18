namespace Pekspro.RadioStorm.Logging;

public sealed class InMemoryLogger : ILogger
{
    private int MaxLinesCount { get; set; } = 1000;

    private int NumberOfLinesToKeep { get; set; } = 500;

    private readonly List<LogRecord> LogRecords = new List<LogRecord>();

    private readonly string Category;

    private IDateTimeProvider DateTimeProvider { get; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public InMemoryLogger(IDateTimeProvider dateTimeProvider, List<LogRecord> logRecords, string category)
    {
        DateTimeProvider = dateTimeProvider;
        LogRecords = logRecords;
        Category = category;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        lock (LogRecords)
        {
            LogRecords.Add(new LogRecord(logLevel, exception, Category, message, DateTimeProvider.OffsetNow));

            if (LogRecords.Count >= MaxLinesCount)
            {
                LogRecords.RemoveRange(0, LogRecords.Count - NumberOfLinesToKeep);
            }
        }
    }
}
