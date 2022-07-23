namespace Pekspro.RadioStorm.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    public FileStream? FileStream { get; }
    
    public IDateTimeProvider DateTimeProvider { get; }

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

    public void Dispose() { }

    internal void Write(byte[] bytes)
    {
        lock (this)
        {
            // Write to file
            FileStream?.Write(bytes);
            FileStream?.Flush();
        }
    }
}
