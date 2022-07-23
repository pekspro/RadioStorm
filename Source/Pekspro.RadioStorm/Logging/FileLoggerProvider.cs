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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
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
