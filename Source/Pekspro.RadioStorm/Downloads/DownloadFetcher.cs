namespace Pekspro.RadioStorm.Downloads;

internal class DownloadFetcher : IDownloadFetcher
{
    public IMessenger Messenger { get; }
    public IDateTimeProvider DateTimeProvider { get; }
    public ILogger Logger { get; }

    private record ActiveDownload(string Url, Download Download, CancellationTokenSource CancellationTokenSource);

    private List<ActiveDownload> ActiveDownloads { get; } = new();

    private Queue<ActiveDownload> QueuedDownloads { get; } = new();

    internal const string DownloadSuffix = ".download";

    private int RunningCount = 0;

    private ActiveDownload? GetNextFromQueue()
    {
        lock (this)
        {
            if (RunningCount >= 2)
            {
                return null;
            }

            if (QueuedDownloads.Count > 0)
            {
                var download = QueuedDownloads.Dequeue();

                RunningCount++;
                
                return download;
            }
            
            return null;
        }
    }

    private void AddToQueue(ActiveDownload download)
    {
        lock (this)
        {
            QueuedDownloads.Enqueue(download);
            ActiveDownloads.Add(download);
        }
    }

    private void CompletedDownload(ActiveDownload download)
    {
        lock (this)
        {
            ActiveDownloads.Remove(download);
            RunningCount--;
        }
    }

    public DownloadFetcher
        (
            IMessenger messenger,
            IDateTimeProvider dateTimeProvider,
            ILogger<DownloadFetcher> logger
        )
    {
        Messenger = messenger;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;
    }

    public void StartDownload(Download downloadData, string url)
    {
        downloadData.Status = DownloadDataStatus.Starting;
        downloadData.BytesDownloaded = 0;
        downloadData.BytesToDownload = 0;
        downloadData.DownloadTime = DateTimeProvider.OffsetNow;

        Messenger.Send(new DownloadAdded(downloadData));
        Messenger.Send(new DownloadUpdated(downloadData));

        var activeDownload = new ActiveDownload(url, downloadData, new CancellationTokenSource());
        AddToQueue(activeDownload);

        GetFromQueueAndDownload();
    }

    private readonly TimeSpan MaxNotificationIntervall = TimeSpan.FromMilliseconds(100);

    public async void GetFromQueueAndDownload()
    {
        ActiveDownload? activeDownload;
        
        while ((activeDownload = GetNextFromQueue()) is not null)
        {
            string tempFileName = activeDownload.Download.Filename + DownloadSuffix;

            Logger.LogInformation($"Downloading {activeDownload.Url} to {tempFileName}.");

            // Do the actual download
            try
            {
                // Download from url to file with HttpClient and keep track of progress
                {
                    using var client = new HttpClient();
                    using var response = await client.GetAsync(activeDownload.Url, HttpCompletionOption.ResponseHeadersRead);
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.Read);

                    ulong latestNotifiedSimplifiedSize = ulong.MaxValue;
                    int latestNotifiedPercentSize = -1;
                    DateTime nextMinNotificationTime = DateTime.MinValue;

                    if (!response.Content.Headers.ContentLength.HasValue)
                    {
                        await stream.CopyToAsync(fileStream, activeDownload.CancellationTokenSource.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        activeDownload.Download.BytesToDownload = (ulong)response.Content.Headers.ContentLength.Value;

                        var buffer = new byte[30 * 1024];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, activeDownload.CancellationTokenSource.Token).ConfigureAwait(false)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, activeDownload.CancellationTokenSource.Token).ConfigureAwait(false);

                            activeDownload.Download.Status = DownloadDataStatus.Downloading;
                            activeDownload.Download.BytesDownloaded += (ulong)bytesRead;

                            // Size in tenth of MB
                            ulong thisSimplifiedSize = (ulong)Math.Round(activeDownload.Download.BytesDownloaded / 104857.6);

                            // Size in percent
                            int thisPercentSize = (int)Math.Round(activeDownload.Download.BytesDownloaded / (double)activeDownload.Download.BytesToDownload * 100);

                            DateTime now = DateTimeProvider.UtcNow;
                            
                            if (now > nextMinNotificationTime)
                            {
                                if (thisSimplifiedSize != latestNotifiedSimplifiedSize || thisPercentSize != latestNotifiedPercentSize)
                                {
                                    Messenger.Send(new DownloadUpdated(activeDownload.Download));
                                    latestNotifiedSimplifiedSize = thisSimplifiedSize;
                                    latestNotifiedPercentSize = thisPercentSize;

                                    nextMinNotificationTime = now.Add(MaxNotificationIntervall);
                                }
                            }
                        }
                    }
                }

                Logger.LogInformation($"Downloaded {activeDownload.Url} to {tempFileName}.");

                // Rename
                File.Move(tempFileName, activeDownload.Download.Filename);

                activeDownload.Download.Status = DownloadDataStatus.Done;
                Messenger.Send(new DownloadUpdated(activeDownload.Download));
            }
            catch (TaskCanceledException ex)
            {
                Logger.LogInformation(ex, "Download cancelled.");

                try
                {
                    File.Delete(tempFileName);
                }
                catch (Exception)
                {

                }

                Messenger.Send(new DownloadDeleted(activeDownload.Download));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Download failed.");
                try
                {
                    File.Delete(tempFileName);
                }
                catch (Exception)
                {

                }

                activeDownload.Download.Status = DownloadDataStatus.Error;
                Messenger.Send(new DownloadUpdated(activeDownload.Download));
            }
        
            CompletedDownload(activeDownload);
        }
    }

    public void StopDownload(Download downloadData)
    {
        lock(this)
        {
            ActiveDownload? activeDownload = ActiveDownloads.FirstOrDefault(x => x.Download == downloadData);

            if (activeDownload is null)
            {
                return;
            }

            activeDownload.CancellationTokenSource.Cancel();
            Messenger.Send(new DownloadDeleted(activeDownload.Download));
        }
    }
}
