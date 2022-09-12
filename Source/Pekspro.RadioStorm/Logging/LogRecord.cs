namespace Pekspro.RadioStorm.Logging;

public sealed record LogRecord(LogLevel LogLevel, Exception? Exception, string Category, string Message, DateTimeOffset Timestamp);
