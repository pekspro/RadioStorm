namespace Pekspro.RadioStorm.Bootstrap;

public sealed class Bootstrap
{
    #region Private properties
    
    private ISettingsService SettingsService { get; }

    private IVersionProvider VersionProvider { get; }

    private GeneralDatabaseHelper GeneralDatabaseHelper { get; }

    private CacheDatabaseHelper CacheDatabaseHelper { get; }

    private IDownloadManager DownloadManager { get; }

    private IServiceProvider ServiceProvider { get; }

    private ILocalSettings LocalSettings { get; }

    private IDownloadSettings DownloadSettings { get; }

    private ILogger Logger { get; }
    
    private IBootstrapState BootstrapState { get; }

    private ISharedSettingsManager SharedSettingsManager { get; }
    
    private ILogFileHelper LogFileHelper { get; }

    private StorageLocations StorageLocation { get; }

    #endregion

    #region Constructor

    public Bootstrap
        (
            ISettingsService settingsService,
            IVersionProvider versionProvider,
            GeneralDatabaseHelper generalDatabaseHelper,
            CacheDatabaseHelper cacheDatabaseHelper,
            IDownloadManager downloadManager,
            IServiceProvider serviceProvider,
            ILocalSettings localSettings,
            IDownloadSettings downloadSettings,
            ILogger<Bootstrap> logger,
            IBootstrapState bootstrapState,
            ISharedSettingsManager sharedSettingsManager,
            IOptions<StorageLocations> storageLocationOptions,
            ILogFileHelper logFileHelper)
    {
        SettingsService = settingsService;
        VersionProvider = versionProvider;
        GeneralDatabaseHelper = generalDatabaseHelper;
        CacheDatabaseHelper = cacheDatabaseHelper;
        DownloadManager = downloadManager;
        ServiceProvider = serviceProvider;
        LocalSettings = localSettings;
        DownloadSettings = downloadSettings;
        Logger = logger;
        BootstrapState = bootstrapState;
        SharedSettingsManager = sharedSettingsManager;
        StorageLocation = storageLocationOptions.Value;
        LogFileHelper = logFileHelper;
    }

    #endregion

    #region Methods

    public async Task SetupAsync(bool forBackgroundTask = false)
    {
        Logger.LogDebug($"Bootstraping starts.");
        Stopwatch watch = Stopwatch.StartNew();

        Directory.CreateDirectory(StorageLocation.BaseStoragePath);
        Directory.CreateDirectory(StorageLocation.LocalSettingsPath);
        Directory.CreateDirectory(StorageLocation.CacheSettingsPath);

        await RunVersionMigrate().ConfigureAwait(false);
        
        SharedSettingsManager.Init(!forBackgroundTask);

        await ReadSettings(forBackgroundTask);

        watch.Stop();

        Logger.LogDebug($"Bootstrap time: {watch.ElapsedMilliseconds} ms.");

        BootstrapState.BootstrapCompleted = true;
    }

    private async Task RunVersionMigrate()
    {
        string latestExecutedVersion = SettingsService.GetSafeValue("LatestExecutedVersion", "");
        string currentVersion = VersionProvider.ApplicationVersion.ToString(); // Utilities.Version.Current.ToString();

        Logger.LogInformation($"Current version: {currentVersion} Last executed version: {latestExecutedVersion}");
        
        bool forceMigrate = false;
        if (!GeneralDatabaseHelper.DatabaseFileExists())
        {
            Logger.LogWarning($"Database file not found for general database.");
            forceMigrate = true;
        }
        
        if (!CacheDatabaseHelper.DatabaseFileExists())
        {
            Logger.LogWarning($"Database file not found for cache database.");
            forceMigrate = true;
        }

        if (currentVersion != latestExecutedVersion || forceMigrate)
        {
            Logger.LogInformation($"Version is changed.");

            await MigrateDatabasesAsync().ConfigureAwait(false);

            var versionMigrator = ServiceProvider.GetService<IVersionMigrator>();

            if (versionMigrator is not null)
            {
                Logger.LogInformation($"Executing version migrator.");
                await versionMigrator.MigrateAsync(latestExecutedVersion, currentVersion).ConfigureAwait(false);
            }

            SettingsService.SetValue("LatestExecutedVersion", currentVersion);
        }
    }

    private async Task MigrateDatabasesAsync()
    {
        Stopwatch gdwatch = Stopwatch.StartNew();

        await GeneralDatabaseHelper.MigrateAsync().ConfigureAwait(false);
        gdwatch.Stop();
        Logger.LogInformation($"Time to migrate general cache database: {gdwatch.ElapsedMilliseconds} ms.");

        Stopwatch cdwatch = Stopwatch.StartNew();
        await CacheDatabaseHelper.MigrateAsync().ConfigureAwait(false);
        cdwatch.Stop();
        Logger.LogInformation($"Time to migrate init cache database: {cdwatch.ElapsedMilliseconds} ms.");
    }

    private async Task ReadSettings(bool forBackgroundTask)
    {
        Stopwatch pdwatch = Stopwatch.StartNew();

        await DownloadManager.InitAsync().ConfigureAwait(false);

        await SharedSettingsManager.ReadLocalSettingsAsync().ConfigureAwait(false);

        DownloadSettings.Init();

        //void DumpSharedSettings()
        //{
        //	void Add(MemoryStream memoryStream, byte[] data)
        //	{
        //		memoryStream.Write(data, 0, data.Length);
        //	}

        //	string Hash(MemoryStream memoryStream)
        //	{
        //		memoryStream.Position = 0;

        //		using (SHA256 mySHA256 = SHA256.Create())
        //		{
        //			var h = mySHA256.ComputeHash(memoryStream);

        //			return BitConverter.ToString(h).Replace("-", "");
        //		}
        //	}

        //	string recentHash;
        //	string listenHash;
        //	string episodeSortHash;
        //	string channelsHash;
        //	string programsHash;
        //	using (MemoryStream memoryStream = new MemoryStream())
        //	{
        //		foreach (var man in ServiceLocator.Current.GetInstance<RecentPlayedManager>().Items.Values.OrderBy(a => a.Id))
        //		{
        //			Add(memoryStream, BitConverter.GetBytes(man.Id));
        //			Add(memoryStream, BitConverter.GetBytes(man.IsEpisode));
        //			Add(memoryStream, BitConverter.GetBytes(man.IsRemoved));
        //			Add(memoryStream, BitConverter.GetBytes(man.Timestamp.Ticks));
        //		}

        //		recentHash = Hash(memoryStream);
        //		memoryStream.Position = 0;
        //		string listenData = BitConverter.ToString(memoryStream.ToArray()).Replace("-", Environment.NewLine);
        //	}

        //	using (MemoryStream memoryStream = new MemoryStream())
        //	{
        //		foreach (var man in ServiceLocator.Current.GetInstance<ListenStateManager>().Items.Values.OrderBy(a => a.Id))
        //		{
        //			Add(memoryStream, BitConverter.GetBytes(man.Id));
        //			Add(memoryStream, BitConverter.GetBytes(man.IsFullyListen));
        //			Add(memoryStream, BitConverter.GetBytes(man.LastChangedTimestamp));
        //			Add(memoryStream, BitConverter.GetBytes(man.ListenLength));
        //		}

        //		listenHash = Hash(memoryStream);
        //		memoryStream.Position = 0;
        //		string listenData = BitConverter.ToString(memoryStream.ToArray()).Replace("-", Environment.NewLine);
        //	}

        //	using (MemoryStream memoryStream = new MemoryStream())
        //	{
        //		foreach (var man in ServiceLocator.Current.GetInstance<EpisodesSortOrderManager>().Items.Values.OrderBy(a => a.Id))
        //		{
        //			Add(memoryStream, BitConverter.GetBytes(man.Id));
        //			Add(memoryStream, BitConverter.GetBytes(man.IsActive));
        //			Add(memoryStream, BitConverter.GetBytes(man.LastChangedTimestamp));
        //		}

        //		episodeSortHash = Hash(memoryStream);
        //		memoryStream.Position = 0;
        //		string listenData = BitConverter.ToString(memoryStream.ToArray()).Replace("-", Environment.NewLine);
        //	}

        //	using (MemoryStream memoryStream = new MemoryStream())
        //	{
        //		foreach (var man in ServiceLocator.Current.GetInstance<FavoriteManager>().Channels.Items.Values.OrderBy(a => a.Id))
        //		{
        //			Add(memoryStream, BitConverter.GetBytes(man.Id));
        //			Add(memoryStream, BitConverter.GetBytes(man.IsActive));
        //			Add(memoryStream, BitConverter.GetBytes(man.LastChangedTimestamp));
        //		}

        //		channelsHash = Hash(memoryStream);
        //		memoryStream.Position = 0;
        //		string listenData = BitConverter.ToString(memoryStream.ToArray()).Replace("-", Environment.NewLine);
        //	}

        //	using (MemoryStream memoryStream = new MemoryStream())
        //	{
        //		foreach (var man in ServiceLocator.Current.GetInstance<FavoriteManager>().Programs.Items.Values.OrderBy(a => a.Id))
        //		{
        //			Add(memoryStream, BitConverter.GetBytes(man.Id));
        //			Add(memoryStream, BitConverter.GetBytes(man.IsActive));
        //			Add(memoryStream, BitConverter.GetBytes(man.LastChangedTimestamp));
        //		}

        //		programsHash = Hash(memoryStream);
        //		memoryStream.Position = 0;
        //		string listenData = BitConverter.ToString(memoryStream.ToArray()).Replace("-", Environment.NewLine);
        //	}
        //}
        //DumpSharedSettings();

        var firstPodInitTime = pdwatch.ElapsedMilliseconds;
        pdwatch.Restart();

        if (!forBackgroundTask)
        {
            RunDelayedSetup();

            if (LocalSettings.MayWantToReview)
            {
                int launchCount = LocalSettings.LaunchCount + 1;
                LocalSettings.LaunchCount = launchCount;

                /*if (launchCount >= LocalSettings.LaunchCountBeforeAskForReview)
                {
                    LocalSettings.AskForReview = true;
                }*/
            }
        }
    }

    private void RunDelayedSetup()
    {
        if (LocalSettings.WriteLogsToFile)
        {
            _ = LogFileHelper.RemoveOldLogFilesAsync(TimeSpan.FromDays(7));
        }

        //
        // var nextTypeToDelete = CacheDatabaseHelper.DeleteObseleteData();
        // Logger.Write("Type to delete from cache database: " + nextTypeToDelete);
    }

    #endregion
}
