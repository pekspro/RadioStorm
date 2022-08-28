namespace Pekspro.RadioStorm.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private bool disposedValue;

    private FileStream? FileStream { get; set; }

    private IDateTimeProvider DateTimeProvider { get; }

    public FileLoggerProvider(IDateTimeProvider dateTimeProvider, FileStream? fileStream)
    {
        DateTimeProvider = dateTimeProvider;
        FileStream = fileStream;
    }

    public static FileLoggerProvider? CreateIfEnabled(IServiceProvider serviceProvider, string logName)
    {
        var localSettings = serviceProvider.GetRequiredService<ILocalSettings>();

        if (!localSettings.WriteLogsToFile)
        {
            return null;
        }

        // Create file name
        var logFileName = serviceProvider.GetRequiredService<ILogFileNameCreator>().CreateLogFilName(logName);

        // Open file
        var fileStream = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read);

        // Create FileLoggerProvder
        var fileLogger = new FileLoggerProvider
            (
                serviceProvider.GetRequiredService<IDateTimeProvider>(),
                fileStream
            );

        // Write header
        fileLogger.Write("Time\tCategory\tLevel\tMessage" + Environment.NewLine, false);

        return fileLogger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Get string after last .
        int lastDot = categoryName.LastIndexOf('.');
        if (lastDot >= 0)
        {
            categoryName = categoryName.Substring(lastDot + 1);
        }

        return new FileLogger(this, categoryName, DateTimeProvider);
    }

    public void Close()
    {
        FileStream?.Close();
        FileStream = null;
    }

    internal void Write(string text, bool flush)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        Write(bytes, false);
    }

    internal void Write(byte[] bytes, bool flush)
    {
        lock (this)
        {
            // Write to file
            FileStream?.Write(bytes);

            if (flush)
            {
                FileStream?.Flush();
            }
        }
    }

    public void Flush()
    {
        lock (this)
        {
            FileStream?.Flush();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Write($"Disposing {nameof(FileLoggerProvider)}", true);

                Close();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
