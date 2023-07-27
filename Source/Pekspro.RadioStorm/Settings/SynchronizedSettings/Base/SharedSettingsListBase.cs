namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;

public abstract class SharedSettingsListBase<T> : ISharedSettingsListBase<T>
{
    public ILogger Logger { get; }
    public int FileCount { get; }

    public Dictionary<int, T> Items { get; set; } = new Dictionary<int, T>();

    private System.Timers.Timer Timer = new ();

    private readonly string LocalSettingsPath;

    public SharedSettingsListBase(ILogger logger, IOptions<StorageLocations> strorageLocationOptions, int fileCount)
    {
        LocalSettingsPath = strorageLocationOptions.Value.LocalSettingsPath;
        Logger = logger;
        FileCount = fileCount;

        Timer.Elapsed += async (s, e) =>
        {
            Timer.Stop();
            await SaveIfDirtyAsync().ConfigureAwait(false);
        };
    }

    protected double TimerInterval
    {
        get
        {
            return Timer.Interval;
        }
        set
        {
            Timer.Interval = value;
        }
    }

    protected void RestartTimer()
    {
        Timer?.Stop();
        Timer?.Start();
    }

    public abstract Task SaveIfDirtyAsync();

    #region LatestChangedTime property

    private DateTime _LatestChangedTime = DateTime.MinValue;

    public virtual DateTime LatestChangedTime
    {
        get
        {
            return _LatestChangedTime;
        }
        protected set
        {
            _LatestChangedTime = value;
        }
    }

    #endregion

    public abstract string GetFileName(int slotId);

    protected abstract Task<Dictionary<int, T>> ReadAsync(ReverseBinaryReader reader);

    protected abstract Task<bool> MergeAsync(Dictionary<int, T> roamingList);

    private static void AddNewItems<A, B>(ref Dictionary<A, B> currentList, Dictionary<A, B> newData) where A : notnull
    {
        if (!currentList.Any())
        {
            currentList = newData;
        }
        else
        {
            foreach (var newitem in newData)
            {
                if (currentList.ContainsKey(newitem.Key))
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    continue;
                }
                else
                {
                    currentList.Add(newitem.Key, newitem.Value);
                }
            }
        }
    }

    public async Task SynchronizeSettingsAsync(SynchronizeSettings synchronizeSettings, List<FileBaseProviderAndFiles> fileBaseProviderAndFiles, HashSet<string> failedProviderNames)
    {
        foreach (var fileBaseProvider in fileBaseProviderAndFiles)
        {
            Dictionary<int, T> data = new Dictionary<int, T>();

            for (int i = 0; i < FileCount; i++)
            {
                string fileName = GetFileName(i);

                if (fileBaseProvider.Files.TryGetValue(fileName, out var fileOverview))
                {
                    if (!fileOverview.IsModified && !synchronizeSettings.ForceRead)
                    {
                        Logger.LogInformation($"{fileName} is not modified in {fileBaseProvider.Provider.Name}. Will not be read and merged.");
                        continue;
                    }
                    else
                    {
                        try
                        {
                            if (!fileOverview.IsModified && synchronizeSettings.ForceRead)
                            {
                                Logger.LogInformation($"Forcing reading of {fileName} in {fileBaseProvider.Provider.Name}. Reading and merging...");
                            }
                            else
                            {
                                Logger.LogInformation($"{fileName} was modified in {fileBaseProvider.Provider.Name}. Reading and merging...");
                            }

                            using (var stream = await fileBaseProvider.Provider.GetFileContentAsync(fileName))
                            {
                                using (var memoryStream = new MemoryStream(10 * 1024))
                                {
                                    await stream.CopyToAsync(memoryStream);
                                    memoryStream.Position = 0;

                                    fileOverview.Checksum = fileBaseProvider.Provider.GetCheckSum(memoryStream);
                                    memoryStream.Position = 0;

                                    data = await PrepareAndReadAsync(data, memoryStream);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error when reading {fileName} from provider {fileBaseProvider.Provider.Name}.", ex);
                            failedProviderNames.Add(fileBaseProvider.Provider.Name);
                        }
                    }
                }
                else
                {
                    // File does not exists
                    Logger.LogInformation($"{fileName} does not exists in {fileBaseProvider.Provider.Name}. Will not be read and merged.");
                }
            }

            if (data.Any())
            {
                await MergeAsync(data).ConfigureAwait(false);
            }
        }

        // Make sure all file providers has the same file content as the local files.
        foreach (var fileBaseProvider in fileBaseProviderAndFiles)
        {
            for (int i = 0; i < FileCount; i++)
            {
                string fileName = GetFileName(i);
                FileOverview? fileOverview;

                fileBaseProvider.Files.TryGetValue(fileName, out fileOverview);

                try
                {
                    await fileBaseProvider.Provider.UpdateFileAsync(fileName, fileOverview).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Synchronizing file {fileName} failed with file provider {fileBaseProvider.Provider.Name}.", ex);
                    failedProviderNames.Add(fileBaseProvider.Provider.Name);
                }
            }
        }
    }

    private async Task<Dictionary<int, T>> PrepareAndReadAsync(Dictionary<int, T> data, Stream stream)
    {
        using (var memoryStream = new MemoryStream(32 * 1024))
        {
            using (GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                await decompressionStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            }

            memoryStream.Position = 0;

            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                ReverseBinaryReader reverseBinaryReader = new ReverseBinaryReader(binaryReader);

                var currentFileData = await ReadAsync(reverseBinaryReader).ConfigureAwait(false);

                AddNewItems(ref data, currentFileData);
            }
        }

        return data;
    }

    public async Task ReadLocalSettingsAsync()
    {
        Items = await ReadAsync(LocalSettingsPath);

        // sharedSettingsLogger.Log(LogName, $"{Items.Count} favorites was read.");
    }

    protected async Task<Dictionary<int, T>> ReadAsync(string folder)
    {
        Dictionary<int, T> data = new Dictionary<int, T>();

        for (int i = 0; i < FileCount; i++)
        {
            string FileName = GetFileName(i);

            try
            {
                string fullPath = Path.Combine(folder, FileName);

                // using (var stream = await folder.OpenStreamForReadAsync(FileName))
                using (var stream = File.OpenRead(fullPath))
                {
                    data = await PrepareAndReadAsync(data, stream);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to read {0}", FileName);
            }
        }

        return data;
    }

    protected async Task SaveAsync(int slotId)
    {
        string filename = GetFileName(slotId);

        Directory.CreateDirectory(LocalSettingsPath);

        try
        {
            // StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            string fullPath = Path.Combine(LocalSettingsPath, filename);

            using (var memoryStream = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    using (var writer = new BinaryWriter(compressionStream))
                    {
                        ReverseBinaryWriter reverseBinaryWriter = new ReverseBinaryWriter(writer);

                        await SaveAsync(slotId, reverseBinaryWriter);
                    }
                }

                byte[] memData = memoryStream.ToArray();

                // using (var fileStream = await storageFile.OpenStreamForWriteAsync())
                using (var fileStream = File.OpenWrite(fullPath))
                {
                    await fileStream.WriteAsync(memData, 0, memData.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to write {0}", filename);
        }
    }

    protected abstract Task SaveAsync(int slotId, ReverseBinaryWriter writer);
}

