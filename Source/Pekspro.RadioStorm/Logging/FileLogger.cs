using System.Text;

namespace Pekspro.RadioStorm.Logging;

internal class FileLogger : ILogger
{
    public FileLoggerProvider FileLoggerProvider { get; }

    public string Category { get; }

    public IDateTimeProvider DateTimeProvider { get; }

    public FileLogger(FileLoggerProvider fileLoggerProvider, string category, IDateTimeProvider dateTimeProvider)
    {
        FileLoggerProvider = fileLoggerProvider;
        Category = category;
        DateTimeProvider = dateTimeProvider;
    }

    public IDisposable BeginScope<TState>(TState state) /* Add in .NET7: where TState : notnull */ => null!;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter is null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        string message = formatter(state, exception);
            
        var logBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(message))
        {
            DateTime timeStamp = DateTimeProvider.LocalNow;
            logBuilder.Append(timeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
            logBuilder.Append('\t');
            logBuilder.Append(Category);
            logBuilder.Append('\t');
            logBuilder.Append(logLevel);
            logBuilder.Append('\t');
            //logBuilder.Append(eventId);
            logBuilder.AppendLine(message);
        }

        if (exception is not null)
        {
            foreach(string exceptionLine in exception.ToString().Split("\r\n".ToCharArray()))
            {
                if (!string.IsNullOrWhiteSpace(exceptionLine))
                {
                    logBuilder.AppendLine($"\t\t\t{exceptionLine.Replace("\t", "    ")}");
                }
            }
        }

        FileLoggerProvider.Write(logBuilder.ToString(), logLevel >= LogLevel.Warning || exception is not null);
    }
}

