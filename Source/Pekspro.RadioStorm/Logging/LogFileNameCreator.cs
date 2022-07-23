namespace Pekspro.RadioStorm.Logging;

internal class LogFileNameCreator : ILogFileNameCreator
{
    private string TemporaryPath { get; }

    private IDateTimeProvider DateTimeProvider { get; }

    public LogFileNameCreator(IOptions<StorageLocations> options, IDateTimeProvider dateTimeProvider)
    {
        TemporaryPath = options.Value.TemporaryPath;
        DateTimeProvider = dateTimeProvider;

        if (!Directory.Exists(TemporaryPath))
        {
            Directory.CreateDirectory(TemporaryPath);
        }
    }

    public string CreateLogFilName(string prefix)
    {
        string filename = $"{prefix}-{DateTimeProvider.OffsetNow.ToString("yyyyMMdd-HHmmss")}.log";

        return Path.Combine(TemporaryPath, filename);
    }
}
