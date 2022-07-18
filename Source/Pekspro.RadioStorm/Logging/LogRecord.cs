namespace Pekspro.RadioStorm.Logging;

public record LogRecord(LogLevel LogLevel, Exception? Exception, string Message, DateTimeOffset Timestamp);
