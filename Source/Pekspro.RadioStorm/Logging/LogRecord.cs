namespace Pekspro.RadioStorm.Logging;

public record LogRecord(LogLevel LogLevel, Exception? Exception, string Category, string Message, DateTimeOffset Timestamp);
