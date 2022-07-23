namespace Pekspro.RadioStorm.Logging;

public interface ILogFileHelper
{
    Task RemoveOldLogFilesAsync();
    Task<string> ZipAllLogFilesAsync();
}