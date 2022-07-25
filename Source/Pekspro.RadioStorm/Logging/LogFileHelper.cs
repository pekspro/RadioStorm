namespace Pekspro.RadioStorm.Logging;

internal class LogFileHelper : ILogFileHelper
{
    private string TemporaryPath { get; }

    private IDateTimeProvider DateTimeProvider { get; }

    public LogFileHelper(IOptions<StorageLocations> options, IDateTimeProvider dateTimeProvider)
    {
        TemporaryPath = options.Value.TemporaryPath;

        if (!Directory.Exists(TemporaryPath))
        {
            Directory.CreateDirectory(TemporaryPath);
        }
        DateTimeProvider = dateTimeProvider;
    }

    public Task RemoveOldLogFilesAsync(TimeSpan minAge)
    {
        return Task.Run(async() =>
        {
            try
            {
                var files = await GetLogFileNamesAsync();
                                
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);

                    if (fileInfo.LastWriteTime < DateTimeProvider.LocalNow.Add(-minAge))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception)
            {

            }
        });
    }

    public async Task<List<string>> GetLogFileNamesAsync()
    {
        List<string> logFileNames = new List<string>();

        await Task.Run(() =>
        {
            try
            {
                logFileNames = Directory.GetFiles(TemporaryPath, "*.log").ToList();
            }
            catch (Exception)
            {

            }
        }).ConfigureAwait(false);

        return logFileNames;
    }

    public async Task<string> ZipAllLogFilesAsync()
    {
        string zipFileName = Path.Combine(TemporaryPath, $"radiostorm-logs.zip");

        // TODO: Proper async support in .NET 7?
        // https://github.com/dotnet/runtime/issues/62658

        await Task.Run(async () =>
        {
            if (File.Exists(zipFileName))
            {
                File.Delete(zipFileName);
            }
            
            using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (string file in Directory.GetFiles(TemporaryPath, "*.log"))
                {
                    try
                    {
                        // Open file and allow share with other processes
                        using var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                        // Add stream to archive
                        var entry = archive.CreateEntry(Path.GetFileName(file));
                        using var entryStream = entry.Open();

                        await stream.CopyToAsync(entryStream);                        
                        
                        //archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                    catch(Exception )
                    {
                        
                    }
                }
            }
        }).ConfigureAwait(false);

        return zipFileName;
    }
}
