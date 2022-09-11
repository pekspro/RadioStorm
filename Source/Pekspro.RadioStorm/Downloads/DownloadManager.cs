namespace Pekspro.RadioStorm.Downloads;

internal class DownloadManager : IDownloadManager
{
    public string DownloadsFolderPath { get; }
    public IDownloadFetcher DownloadFetcher { get; }
    public IMessenger Messenger { get; }
    public ILogger<DownloadManager> Logger { get; }

    private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

    private const int SemaphoreTimeout = 20;

    private List<Download> DownloadDatas { get; } = new List<Download>();

    public DownloadManager
        (
            IDownloadFetcher downloadFetcher,
            IMessenger messenger,
            ILogger<DownloadManager> logger,
            IOptions<StorageLocations> strorageLocationOptions
        )
    {
        DownloadsFolderPath = Path.Combine(strorageLocationOptions.Value.CacheSettingsPath, "downloads");
        DownloadFetcher = downloadFetcher;
        Messenger = messenger;
        Logger = logger;
    }

    public Task InitAsync()
    {
        CreateDownloadFolder();
        ReadFilesFromFolderAsync();

        return Task.CompletedTask;
    }

    private void CreateDownloadFolder()
    {
        if (!Directory.Exists(DownloadsFolderPath))
        {
            Directory.CreateDirectory(DownloadsFolderPath);
        }
    }

    private void ReadFilesFromFolderAsync()
    {
        // Get all files from folder
        var filenames = Directory.GetFiles(DownloadsFolderPath, "*.mp3");
        foreach (var filename in filenames)
        {
            // Check if matching pattern with regex
            // digits-digits.mp3

            var match = Regex.Match(filename, @"(\d+)-(\d+)\.mp3");
            // If match, convert digits to long
            if (match.Success)
            {
                try
                {
                    var programId = int.Parse(match.Groups[1].Value);
                    var episodeId= int.Parse(match.Groups[2].Value);

                    // Get file size
                    var fileInfo = new FileInfo(filename);

                    DownloadDatas.Add(new Download(filename, programId, episodeId, false, null, null)
                    {
                        Status = DownloadDataStatus.Done,
                        BytesDownloaded = (ulong) fileInfo.Length,
                        BytesToDownload = (ulong) fileInfo.Length,
                        DownloadTime = fileInfo.LastWriteTime
                    });
                }
                catch (Exception)
                {
                    // Corrupt filename, delete it
                    Logger.LogWarning($"Could not parse filename {filename}");
                    File.Delete(filename);
                }
            }
            else
            {
                // If no match, delete file
                Logger.LogWarning($"Could not parse filename {filename}");
                File.Delete(filename);
            }
        }

        // Delete files with the download suffix.
        foreach (var filename in Directory.GetFiles(DownloadsFolderPath, "*" + Downloads.DownloadFetcher.DownloadSuffix))
        {
            try
            {
                File.Delete(filename);
                Logger.LogWarning($"Deleted file {filename}.");
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, $"Could not delete file {filename}.");
            }
        }
    }

    private string GetFileName(int programId, int episodeId)
    {
        return $"{programId}-{episodeId}.mp3";
    }

    public Download? GetDownloadData(int programId, int episodeId)
    {
        return DownloadDatas.FirstOrDefault(x => x.ProgramId == programId && x.EpisodeId == episodeId);
    }

    public List<Download> GetDownloads()
    {
        return DownloadDatas.ToList();
    }

    public List<Download> GetActiveUserDownloads()
    {
        return DownloadDatas
                    .Where(a => a.StartedByUser == true)
                    .Where(a => a.Status is DownloadDataStatus.Starting or DownloadDataStatus.Downloading or DownloadDataStatus.Paused)
                    .ToList();
    }

    public async void StartDownload(int programId, int episodeId, string url, bool startedByUser, string? programName, string? episodeTitle)
    {
        if (!await Semaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreTimeout)).ConfigureAwait(false))
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Logger.LogInformation($"Got timeout waiting for semaphore in ({StartDownload}).");

            return;
        }

        try
        {
            var downloadData = GetDownloadData(programId, episodeId);
            bool newDownload = false;
            if (downloadData is null)
            {
                newDownload = true;
                string filename = Path.Combine(DownloadsFolderPath, GetFileName(programId, episodeId));
                downloadData = new Download(filename, programId, episodeId, startedByUser, programName, episodeTitle);
                DownloadDatas.Add(downloadData);
            }

            if (newDownload || downloadData.Status == DownloadDataStatus.Error)
            {
                DownloadFetcher.StartDownload(downloadData, url);
            }
        }
        finally
        {
            Semaphore.Release();
        }

    }

    public async void DeleteDownload(int programId, int episodeId)
    {
        if (!await Semaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreTimeout)).ConfigureAwait(false))
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Logger.LogInformation($"Got timeout waiting for semaphore in ({DeleteDownload}).");

            return;
        }

        try
        {
            var downloadData = GetDownloadData(programId, episodeId);
            if (downloadData is not null)
            {
                DownloadDatas.Remove(downloadData);
            
                if (downloadData.Status is DownloadDataStatus.Done or DownloadDataStatus.Error)
                {
                    try
                    {
                        File.Delete(downloadData.Filename);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Could not delete file.");
                    }
                }
                else
                {
                    DownloadFetcher.StopDownload(downloadData);
                }

                Messenger.Send(new DownloadDeleted(downloadData));
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task ShutDownAsync()
    {
        if (!await Semaphore.WaitAsync(TimeSpan.FromSeconds(SemaphoreTimeout)).ConfigureAwait(false))
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Logger.LogInformation($"Got timeout waiting for semaphore in ({ShutDownAsync}).");

            return;
        }

        try
        {
            for (int i = DownloadDatas.Count - 1; i >= 0; i--)
            {
                if (DownloadDatas[i].Status is DownloadDataStatus.Starting or DownloadDataStatus.Downloading or DownloadDataStatus.Paused)
                {
                    DownloadFetcher.StopDownload(DownloadDatas[i]);
                    DownloadDatas.RemoveAt(i);
                }
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
