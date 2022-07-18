namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider;

public class FileProvidersManager : IFileProvidersManager
{
    public FileProvidersManager(
                IGraphHelper graphHelper,
                GraphFileProvider graphFileProvider,
                ISharedSettingsManager sharedSettingsManager,
                IMessenger messenger,
                IBootstrapState bootstrapState,
                ILogger<FileProvidersManager> logger
                )
    {
        GraphHelper = graphHelper;
        GraphFileProvider = graphFileProvider;
        SharedSettingsManager = sharedSettingsManager;
        BootstrapState = bootstrapState;
        Logger = logger;

        messenger.Register<UiLoaded>(this, (r, m) =>
        {
            messenger.Unregister<UiLoaded>(this);

            Logger.LogDebug($"UI loaded. Initializing file providers...");

            _ = InitAsync();
        });
    }

    private IGraphHelper GraphHelper { get; }
    private GraphFileProvider GraphFileProvider { get; }
    private ISharedSettingsManager SharedSettingsManager { get; }
    public IBootstrapState BootstrapState { get; }
    private ILogger<FileProvidersManager> Logger { get; }
    private bool IsInitialized { get; set; }

    private volatile bool IsInitialing = false;

    public async void InitWithDelay()
    {
        await Task.Delay(3000);

        Logger.LogDebug($"Waited enough. Initializing file providers...");

        await InitAsync();
    }

    public async Task InitAsync()
    {
        if (IsInitialized || IsInitialing)
        {
            Logger.LogDebug($"File providers already initialized.");
            return;
        }

        IsInitialing = true;

        Stopwatch sp = Stopwatch.StartNew();
        Logger.LogInformation($"Initiliazing file providers...");

        if (GraphHelper.IsConfigured)
        {
            await GraphHelper.InitAsync();

            SharedSettingsManager.RegisterFilerProvider(GraphFileProvider);

            _ = GraphHelper.SignInViaCacheAsync();
        }

        IsInitialized = true;
        BootstrapState.FileProvidersInitialized = true;

        Logger.LogInformation($"Initiliazing file providers completed in {sp.Elapsed} ms.");

        // Wait a bit.
        await Task.Delay(5000);

        // Sync fast file providers.
        await SharedSettingsManager.SynchronizeSettingsAsync(new SynchronizeSettings(false, true)).ConfigureAwait(false);
    }
}
