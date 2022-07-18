namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base
{
    public abstract class FileBaseProvider : IFileProvider
    {
        protected const string SyncPath = "SynchronizingData1";
        protected abstract string SyncType { get; }

        public string Name { get; }
        public ISettingsService SettingsService { get; }
        public ILogger Logger { get; }

        public virtual bool IsReady { get; } = true;

        public virtual bool IsSlow { get; } = false;

        private string LocalSettingsPath;

        public FileBaseProvider(string name, ISettingsService settingsService, ILogger logger,
            IOptions<StorageLocations> storageLocationsOptions)
        {
            Name = name;
            SettingsService = settingsService;
            Logger = logger;
            LocalSettingsPath = storageLocationsOptions.Value.LocalSettingsPath;
        }

        private string GetFileState(DateTimeOffset? lastModified, long? fileSize)
        {
            return $"{(!lastModified.HasValue ? "null" : lastModified.Value.UtcDateTime.Ticks.ToString())}_{fileSize}";
        }

        private string GetFileStateName(string name)
        {
            return $"{SyncPath}_{SyncType}_State_{name}";
        }

        private string GetAppSettingsChecksumName(string name)
        {
            return $"{SyncPath}_{SyncType}_Checksum_{name}";
        }

        protected void UpdateFileState(string filename, DateTimeOffset? lastModified, long? fileSize)
        {
            string fileStateName = GetFileStateName(filename);
            string state = GetFileState(lastModified, fileSize);

            SettingsService.SetValue(fileStateName, state);
        }

        protected void UpdateFileState(string filename, string checksum)
        {
            string checksumStateName = GetAppSettingsChecksumName(filename);

            SettingsService.SetValue(checksumStateName, checksum);
        }

        protected string GetStoredChecksum(string filename)
        {
            string checksumStateName = GetAppSettingsChecksumName(filename);

            //var obj = ApplicationData.Current.LocalSettings.Values[checksumStateName];
            var obj = SettingsService.GetValue<string>(checksumStateName);

            if (obj is null)
            {
                return string.Empty;
            }
            else
            {
                return obj.ToString();
            }
        }

        protected void UpdateFileState(string filename, DateTimeOffset? lastModified, long? fileSize, string checksum)
        {
            UpdateFileState(filename, checksum);
            UpdateFileState(filename, lastModified, fileSize);
        }

        protected bool IsModified(string name, DateTimeOffset? lastModified, long? fileSize)
        {
            string fileStateName = GetFileStateName(name);

            var savedState = SettingsService.GetValue<string>(fileStateName);
            if (savedState is null)
            {
                return true;
            }

            string currentState = GetFileState(lastModified, fileSize);

            return currentState != savedState.ToString();
        }

        protected bool IsModified(string name, string checksum)
        {
            string checksumStateName = GetAppSettingsChecksumName(name);

            var savedState = SettingsService.GetValue<string>(checksumStateName);
            if (savedState is null)
            {
                return true;
            }

            return checksum != savedState.ToString();
        }

        public string GetCheckSum(Stream instream)
        {
            var pos = instream.Position;

            var sha = SHA256.Create();
            byte[] checksum = sha.ComputeHash(instream);
            instream.Position = pos;

            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        public async Task<bool> UpdateFileIfNewerAsync(string fileName)
        {
            string localPath = Path.Combine(LocalSettingsPath, fileName);

            using (var stream = File.OpenRead(localPath))
            {
                FileOverview fileData = await FetchFileOverviewAsync(fileName);

                if (fileData.Exists == false || fileData.IsModified == false)
                {
                    string checksum = GetCheckSum(stream);

                    Stopwatch stopwatch = new Stopwatch();

                    var newFileInfo = await UploadFileAsync(fileName, stream);

                    UpdateFileState(fileName, newFileInfo.DateModified, newFileInfo.Size, checksum);

                    Logger.Log(LogLevel.Information, $"Modified file {fileName} updated in {stopwatch.ElapsedMilliseconds} ms.");

                    return true;
                }
                else
                {
                    Logger.Log(LogLevel.Information, $"Will not update remote file {fileName}. Remote has been modified.");

                    return false;
                }
            }
        }

        public async Task UpdateFileAsync(string fileName, FileOverview? fileData)
        {
            try
            {
                // using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(fileName))

                string localPath = Path.Combine(LocalSettingsPath, fileName);

                using (var stream = File.OpenRead(localPath))
                {
                    string checksum = GetCheckSum(stream);

                    // Upload if any of:
                    // File does not exists in remote: fileData is null
                    // Remote checksum does not match local checksum.
                    if (fileData is null || fileData.Checksum != checksum)
                    {
                        Stopwatch stopwatch = new Stopwatch();

                        var newFileInfo = await UploadFileAsync(fileName, stream);

                        UpdateFileState(fileName, newFileInfo.DateModified, newFileInfo.Size, checksum);

                        Logger.Log(LogLevel.Information, $"Modified file {fileName} updated in {stopwatch.ElapsedMilliseconds} ms.");
                    }
                    else
                    {
                        UpdateFileState(fileName, fileData.LastModifiedDateTime, fileData.Size, checksum);

                        Logger.Log(LogLevel.Information, $"File {fileName} was not updated, already synchronized.");
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // Local file does not exists. Silently ignore.
            }
        }

        /// <summary>
        /// Get file from file provider.
        /// </summary>
        /// <param name="allowedLowerCaseFilename">Valid lowercase filename</param>
        /// <returns>Dictionary of files.</returns>
        public abstract Task<Dictionary<string, FileOverview>> GetFilesAsync(HashSet<string> allowedLowerCaseFilename);

        /// <summary>
        /// Fetch file overview from file provider
        /// </summary>
        /// <param name="filename">Filename of file to fetch.</param>
        /// <returns>File overview.</returns>
        public abstract Task<FileOverview> FetchFileOverviewAsync(string filename);

        public abstract Task<Stream> GetFileContentAsync(string fileName);

        protected abstract Task<(DateTimeOffset DateModified, long Size)> UploadFileAsync(string fileName, Stream stream);


    }
}
