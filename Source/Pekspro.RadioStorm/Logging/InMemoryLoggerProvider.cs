namespace Pekspro.RadioStorm.Logging;

public sealed class InMemLoggerProvider : ILoggerProvider
{
    private readonly List<LogRecord> _LogRecords = new List<LogRecord>();

    public InMemLoggerProvider(IDateTimeProvider dateTimeProvider)
    {
        DateTimeProvider = dateTimeProvider;
    }
    
    public IEnumerable<LogRecord> LogRecords => _LogRecords.AsReadOnly();

    public IDateTimeProvider DateTimeProvider { get; }

    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryLogger(DateTimeProvider, _LogRecords, categoryName);
    }

    public void Dispose() { }
}
