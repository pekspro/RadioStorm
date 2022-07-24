namespace Pekspro.RadioStorm.Logging;

public interface ILogFileHelper
{
    Task<List<string>> GetLogFileNamesAsync();
    Task RemoveOldLogFilesAsync(TimeSpan minAge);

    Task<string> ZipAllLogFilesAsync();
}