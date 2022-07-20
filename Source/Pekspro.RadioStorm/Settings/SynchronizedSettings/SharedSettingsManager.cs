namespace Pekspro.RadioStorm.Settings.SynchronizedSettings;

public class SharedSettingsManager : ISharedSettingsManager
{
    private IListenStateManager ListenStateManager { get; }
    public IMessenger Messenger { get; }
    public IChannelFavoriteList ChannelFavorites { get; }
    public IProgramFavoriteList ProgramFavorites { get; }

    private IEpisodesSortOrderManager EpisodesSortOrderManager { get; }
    private IRecentPlayedManager RecentPlayedManager { get; }
    public IDateTimeProvider DateTimeProvider { get; }
    private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1);

    private static readonly TimeSpan SemaphoreTimeout = TimeSpan.FromSeconds(15);

    public bool AllowBackgroundSaving { get; private set; }

    private HashSet<string> AllowedLowerCaseFileNames = new HashSet<string>();

    public SharedSettingsState SharedSettingsState { get; set; } = null!;

    private List<IFileProvider> RemoteFileProviders { get; } = new List<IFileProvider>();
    private IEnumerable<IFileProvider> SafeRemoteFileProviders => RemoteFileProviders.ToArray();
    public ILogger Logger { get; }

    public bool HasAnyRemoteSignedInProvider => SafeRemoteFileProviders.Any(x => x.IsSlow && x.IsReady);

    #region IsSynchronizing property

    private bool _IsSynchronizing;

    public bool IsSynchronizing
    {
        get
        {
            return _IsSynchronizing;
        }
        set
        {
            if (_IsSynchronizing != value)
            {
                _IsSynchronizing = value;

                Messenger.Send(new SynchronizingStateChanged(value));
            }
        }
    }

    #endregion

    public DateTimeOffset? LatestSynchronizingTime { get; set; }

    public SharedSettingsManager(
        IMessenger messenger,
        IChannelFavoriteList channelFavorites,
        IProgramFavoriteList programFavorites,
        IListenStateManager listenStateManager,
        IEpisodesSortOrderManager episodesSortOrderManager,
        IRecentPlayedManager recentPlayedManager,
        IDateTimeProvider dateTimeProvider,
        ILogger<SharedSettingsManager> logger
        )
    {
        Messenger = messenger;
        ChannelFavorites = channelFavorites;
        ProgramFavorites = programFavorites;
        ListenStateManager = listenStateManager;
        EpisodesSortOrderManager = episodesSortOrderManager;
        RecentPlayedManager = recentPlayedManager;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;

        messenger.Register<LocalSharedFileUpdated>(this, (r, m) =>
        {
            _ = UpdateRemoteFileIfNewerAsync(m.Filename);
        });

        messenger.Register<ProviderStateChangedEventArgs>(this, (r, m) =>
        {
            if (m.NewState == ProviderState.SignedIn)
            {
                _ = SynchronizeSettingsAsync(new SynchronizeSettings(true, false));
            }
        });
    }

    public void RegisterFilerProvider(IFileProvider fileProvider)
    {
        RemoteFileProviders.Add(fileProvider);
    }

    public void Init(bool allowBackgroundSaving)
    {
        if (allowBackgroundSaving)
        {
            SharedSettingsState = new SharedSettingsState();
        }

        AllowBackgroundSaving = allowBackgroundSaving;

        ChannelFavorites.Init(AllowBackgroundSaving);
        ProgramFavorites.Init(AllowBackgroundSaving);
        EpisodesSortOrderManager.Init(AllowBackgroundSaving);
        ListenStateManager.Init(AllowBackgroundSaving);
        RecentPlayedManager.Init(AllowBackgroundSaving);

        AllowedLowerCaseFileNames = new HashSet<string>();
        for (int i = 0; i < ChannelFavorites.FileCount; i++)
        {
            AllowedLowerCaseFileNames.Add(ChannelFavorites.GetFileName(i));
        }

        for (int i = 0; i < ProgramFavorites.FileCount; i++)
        {
            AllowedLowerCaseFileNames.Add(ProgramFavorites.GetFileName(i));
        }

        for (int i = 0; i < EpisodesSortOrderManager.FileCount; i++)
        {
            AllowedLowerCaseFileNames.Add(EpisodesSortOrderManager.GetFileName(i));
        }

        for (int i = 0; i < ListenStateManager.FileCount; i++)
        {
            AllowedLowerCaseFileNames.Add(ListenStateManager.GetFileName(i));
        }

        for (int i = 0; i < RecentPlayedManager.FileCount; i++)
        {
            AllowedLowerCaseFileNames.Add(RecentPlayedManager.GetFileName(i));
        }
    }

    public async Task ReadLocalSettingsAsync()
    {
        Logger.LogInformation("Reading local settings.");
        Stopwatch totalwatch = Stopwatch.StartNew();
        Stopwatch stopwatch = Stopwatch.StartNew();

        void LogTime(string t)
        {
            Logger.LogInformation($"{t} completed in {stopwatch.ElapsedMilliseconds} ms.");
            stopwatch.Restart();
        }

        await ChannelFavorites.ReadLocalSettingsAsync();
        LogTime(nameof(ChannelFavorites));

        await ProgramFavorites.ReadLocalSettingsAsync();
        LogTime(nameof(ProgramFavorites));

        await EpisodesSortOrderManager.ReadLocalSettingsAsync();
        LogTime(nameof(EpisodesSortOrderManager));

        await ListenStateManager.ReadLocalSettingsAsync();
        LogTime(nameof(ListenStateManager));

        if (AllowBackgroundSaving)
        {
            await RecentPlayedManager.ReadLocalSettingsAsync();
            LogTime(nameof(RecentPlayedManager));
        }

        totalwatch.Stop();
        Logger.LogInformation($"Reading local settings completed in {totalwatch.ElapsedMilliseconds} ms.");
    }

    public async Task ForceSaveNowAsync()
    {
        List<Task> tasks = new List<Task>
        {
            ListenStateManager.SaveIfDirtyAsync(),
            RecentPlayedManager.SaveIfDirtyAsync(),
            ChannelFavorites.SaveIfDirtyAsync(),
            ProgramFavorites.SaveIfDirtyAsync(),
            EpisodesSortOrderManager.SaveIfDirtyAsync()
        };

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task SynchronizeSettingsAsync(SynchronizeSettings synchronizeSettings)
    {
        Logger.LogInformation("Synchronizing settings. Waiting for access.");

        if (!await Semaphore.WaitAsync(SemaphoreTimeout).ConfigureAwait(false))
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Logger.LogInformation("Got timeout waiting for semaphore in SynchronizeSettingsAsync.");

            return;
        }

        IsSynchronizing = true;

        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<FileBaseProviderAndFiles> fileBaseProviderAndFiles = new List<FileBaseProviderAndFiles>();

            List<(IFileProvider Provider, Task<Dictionary<string, FileOverview>> GetFileTask)> providerTasks =
                new List<(IFileProvider Provider, Task<Dictionary<string, FileOverview>> GetFileTask)>();

            foreach (var provider in SafeRemoteFileProviders)
            {
                if (provider.IsSlow && !synchronizeSettings.UseSlowProviders)
                {
                    continue;
                }

                if (!provider.IsReady)
                {
                    continue;
                }


                Logger.LogInformation($"Getting files from {provider.Name}.");

                Task<Dictionary<string, FileOverview>>? task = provider.GetFilesAsync(AllowedLowerCaseFileNames);

                providerTasks.Add((provider, task));
            }

            var delayTask = Task.Delay(5000);
            var waitAllTask = Task.WhenAll(providerTasks.Select(t => t.GetFileTask));
            await Task.WhenAny(delayTask, waitAllTask).ConfigureAwait(false);

            foreach (var providerTask in providerTasks)
            {
                if (providerTask.GetFileTask.Status == TaskStatus.RanToCompletion && providerTask.GetFileTask.Result is not null)
                {
                    Logger.LogInformation($"Got {providerTask.GetFileTask.Result.Count} files from {providerTask.Provider.Name}.");

                    fileBaseProviderAndFiles.Insert(0, new FileBaseProviderAndFiles(providerTask.Provider, providerTask.GetFileTask.Result));
                }
                else
                {
                    Logger.LogWarning($"Failed to get files from {providerTask.Provider.Name}.");
                }
            }

            await ChannelFavorites.SynchronizeSettingsAsync(synchronizeSettings, fileBaseProviderAndFiles).ConfigureAwait(false);
            await ProgramFavorites.SynchronizeSettingsAsync(synchronizeSettings, fileBaseProviderAndFiles).ConfigureAwait(false);
            await EpisodesSortOrderManager.SynchronizeSettingsAsync(synchronizeSettings, fileBaseProviderAndFiles).ConfigureAwait(false);
            await ListenStateManager.SynchronizeSettingsAsync(synchronizeSettings, fileBaseProviderAndFiles).ConfigureAwait(false);
            await RecentPlayedManager.SynchronizeSettingsAsync(synchronizeSettings, fileBaseProviderAndFiles).ConfigureAwait(false);

            Logger.LogInformation($"Synchronizing settings completed in {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally
        {
            Semaphore.Release();
        }

        LatestSynchronizingTime = DateTimeProvider.OffsetNow;
        IsSynchronizing = false;
    }

    private async Task UpdateRemoteFileIfNewerAsync(string fileName)
    {
        Logger.LogInformation($"Update file {fileName} on remote if newer. Waiting for access.");

        if (!await Semaphore.WaitAsync(SemaphoreTimeout).ConfigureAwait(false))
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Logger.LogInformation("Got timeout waiting for semaphore in UpdateRemoteFileIfNewerAsync.");

            return;
        }

        Logger.LogInformation($"Update file {fileName} on remote if newer.");
        bool successFull = true;

        /*
        try
        {
            WindowsRoamingFileProvider windowsRoamingFileProvider = new WindowsRoamingFileProvider();
            successFull = successFull & await windowsRoamingFileProvider.UpdateFileIfNewerAsync(fileName, SharedSettingsLogger);

            if (ProviderManager.Instance.GlobalProvider?.State == ProviderState.SignedIn)
            {
                SharedSettingsLogger.Log(LogName, "Will try on update on Graph.");

                GraphFileProvider graphFileProvider = new GraphFileProvider();
                successFull = successFull & await graphFileProvider.UpdateFileIfNewerAsync(fileName, SharedSettingsLogger);
            }

            SharedSettingsLogger.Log(LogName, $"Update file {fileName} on remote if newer completed.");
        }
        finally
        {
            Semaphore.Release();
        }
        */

        try
        {
            foreach (var provider in SafeRemoteFileProviders)
            {
                if (provider.IsReady)
                {
                    try
                    {
                        bool updateResult = await provider.UpdateFileIfNewerAsync(fileName);

                        if (updateResult)
                        {
                            Logger.LogInformation($"{fileName} was updated in {provider.Name}.");
                        }
                        else
                        {
                            Logger.LogWarning($"{fileName} was not updated in {provider.Name}. Provider has a newer version.");
                            successFull = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"{fileName} was could not be updated in {provider.Name}. An error occured: {ex.Message}");
                    }
                }
            }
        }
        finally
        {
            Semaphore.Release();
        }

        if (!successFull)
        {
            Logger.LogInformation($"Will trigger full synchronizing.");

            await SynchronizeSettingsAsync(new SynchronizeSettings(true, false)).ConfigureAwait(false);
        }
    }
}
