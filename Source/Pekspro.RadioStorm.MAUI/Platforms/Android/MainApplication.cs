using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Services;
using Intent = Android.Content.Intent;

[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]

namespace Pekspro.RadioStorm.MAUI;

[Application]
public sealed class MainApplication : MauiApplication
{
    sealed class MediaPlayerServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly MainApplication TheApplication;

        public MediaPlayerServiceConnection(MainApplication mainApplication)
        {
            TheApplication = mainApplication;
        }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            if (service is MediaPlayerServiceBinder mediaPlayerServiceBinder)
            {
                TheApplication.MediaPlayerServiceBinder = mediaPlayerServiceBinder;

                TheApplication.OnMediaServiceStarted(mediaPlayerServiceBinder.GetMediaPlayerService());
            }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            TheApplication.MediaPlayerServiceBinder = null;

            ((AndroidAudioManager)ServiceProviderHelper.GetRequiredService<IAudioManager>()).MediaPlayerService = null!;
        }
    }

    sealed class DownloadServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly MainApplication TheApplication;

        public DownloadServiceConnection(MainApplication mainApplication)
        {
            TheApplication = mainApplication;
        }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            if (service is DownloadServiceBinder downloadServiceBinder)
            {
                TheApplication.DownloadServiceBinder = downloadServiceBinder;
            }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            TheApplication.DownloadServiceBinder = null;
        }
    }

    sealed class SleepTimerServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly MainApplication TheApplication;

        public SleepTimerServiceConnection(MainApplication mainApplication)
        {
            TheApplication = mainApplication;
        }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            if (service is SleepTimerServiceBinder sleepTimerServiceBinder)
            {
                TheApplication.SleepTimerServiceBinder = sleepTimerServiceBinder;
            }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            TheApplication.SleepTimerServiceBinder = null;
        }
    }

    private MediaPlayerServiceConnection MediaPlayerServiceConnectionObject { get; }
    private MediaPlayerServiceBinder? MediaPlayerServiceBinder;

    private DownloadServiceConnection DownloadServiceConnectionObject { get; }
    private DownloadServiceBinder? DownloadServiceBinder;

    private SleepTimerServiceConnection SleepTimerServiceConnectionObject { get; }
    private SleepTimerServiceBinder? SleepTimerServiceBinder;

    private ConnectivityManager? connectivityManager;

    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;

        NotificationHelper.StopNotification(this);

        DownloadServiceConnectionObject = new DownloadServiceConnection(this);
        SleepTimerServiceConnectionObject = new SleepTimerServiceConnection(this);
        MediaPlayerServiceConnectionObject = new MediaPlayerServiceConnection(this);
    }

    public override void OnCreate()
    {
        base.OnCreate();

        connectivityManager = (ConnectivityManager?) GetSystemService(ConnectivityService);
        connectivityManager?.RegisterDefaultNetworkCallback(new ConnectivityMonitor());

        IMessenger? messenger = ServiceProviderHelper.GetService<IMessenger>();

        if (messenger is not null)
        {
            messenger?.Register<DownloadAdded>(this, (r, m) =>
            {
                UpdateDownloadServiceExpectedStatus();
            });

            messenger?.Register<DownloadUpdated>(this, (r, m) =>
            {
                UpdateDownloadServiceExpectedStatus();
            });

            messenger?.Register<DownloadDeleted>(this, (r, m) =>
            {
                UpdateDownloadServiceExpectedStatus();
            });

            messenger?.Register<SleepStateChanged>(this, (r, m) =>
            {
                UpdateSleepTimerServiceExpectedStatus(m);
            });
        }

        ((AndroidAudioManager)ServiceProviderHelper.GetRequiredService<IAudioManager>()).MediaServiceStarter = MediaServiceStarter;
    }

    private Action<MediaPlayerService>? MediaServiceStartedCallback;

    private bool MediaServiceRequestIsExecuted = false;

    private Stopwatch? MediaServiceStarterStopWatch;

    public void MediaServiceStarter(Action<MediaPlayerService>? mediaServiceStartedCallback)
    {
        MediaServiceStartedCallback = mediaServiceStartedCallback;

        if (MediaServiceRequestIsExecuted)
        {
            Logger.LogWarning("Has got multiple request to start media service.");

        }
        else
        {
            MediaServiceRequestIsExecuted = true;
            MediaServiceStarterStopWatch = Stopwatch.StartNew();

            Logger.LogInformation("Get request to start media service.");

            NotificationHelper.CreateNotificationChannel(ApplicationContext);
            var mediaPlayerServiceIntent = new Intent(this, typeof(MediaPlayerService));
            BindService(mediaPlayerServiceIntent, MediaPlayerServiceConnectionObject, Bind.AutoCreate);
        }
    }
    
    private void OnMediaServiceStarted(MediaPlayerService mediaService)
    {
        Logger.LogInformation($"Media service started. Time to start service {MediaServiceStarterStopWatch?.Elapsed}");

        ((AndroidAudioManager)ServiceProviderHelper.GetRequiredService<IAudioManager>()).MediaPlayerService = mediaService;

        MediaServiceStartedCallback?.Invoke(mediaService);
    }

    private void UpdateDownloadServiceExpectedStatus()
    {
        IMainThreadRunner? mainThreadRunner = ServiceProviderHelper.GetService<IMainThreadRunner>();
        IDownloadManager? downloadManager = ServiceProviderHelper.GetService<IDownloadManager>();

        if (mainThreadRunner is not null && downloadManager is not null)
        {
            mainThreadRunner.RunInMainThread(() =>
            {
                var downloads = downloadManager.GetActiveUserDownloads();

                if (downloads.Any())
                {
                    StartDownloadService();
                    
                    DownloadServiceBinder?.GetDownloadService()?.UpdateNotification();
                }
                else
                {
                    StopDownloadService();
                }
            });
        }
    }

    private void StartDownloadService()
    {
        if (DownloadServiceBinder is not null)
        {
            Logger.LogDebug("Download service already started.");
            return;
        }
        
        Intent intent = new Intent(this, typeof(DownloadService));
        bool ret = BindService(intent, DownloadServiceConnectionObject, Bind.AutoCreate);

        if (ret)
        {
            Logger.LogInformation("Download service start request.");
        }
        else
        {
            Logger.LogError("Failed to start download service.");
        }
    }

    private void StopDownloadService()
    {
        if (DownloadServiceBinder is null)
        {
            Logger.LogDebug("Download service is not running.");
            return;
        }

        Logger.LogInformation("Stopping download service.");
        
        UnbindService(DownloadServiceConnectionObject);
        DownloadServiceBinder = null;
    }


    private void UpdateSleepTimerServiceExpectedStatus(SleepStateChanged message)
    {
        IMainThreadRunner? mainThreadRunner = ServiceProviderHelper.GetService<IMainThreadRunner>();
        
        if (mainThreadRunner is not null)
        {
            mainThreadRunner.RunInMainThread(() =>
            {
                if (message.IsSleepModeActivated)
                {
                    StartSleepTimerService();

                    SleepTimerServiceBinder?.GetSleepTimerService()?.UpdateNotification(message.TimeLeftToSleepActivation);
                }
                else
                {
                    StopSleepTimerService();
                }
            });
        }
    }

    private void StartSleepTimerService()
    {
        if (SleepTimerServiceBinder is not null)
        {
            Logger.LogDebug("SleepTimer service already started.");
            return;
        }

        Intent intent = new Intent(this, typeof(SleepTimerService));
        bool ret = BindService(intent, SleepTimerServiceConnectionObject, Bind.AutoCreate);

        if (ret)
        {
            Logger.LogInformation("SleepTimer service start request.");
        }
        else
        {
            Logger.LogError("Failed to start SleepTimer service.");
        }
    }

    private void StopSleepTimerService()
    {
        if (SleepTimerServiceBinder is null)
        {
            Logger.LogDebug("SleepTimer service is not running.");
            return;
        }

        Logger.LogInformation("Stopping SleepTimer service.");

        UnbindService(SleepTimerServiceConnectionObject);
        SleepTimerServiceBinder.GetSleepTimerService().StopSelf();
        SleepTimerServiceBinder = null;
    }

    private ILogger? _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= ServiceProviderHelper.GetRequiredService<ILogger<App>>();
        }
    }
        
    private void AndroidEnvironment_UnhandledExceptionRaiser(object? sender, RaiseThrowableEventArgs e)
    {
        Logger.LogError(e.Exception, nameof(AndroidEnvironment_UnhandledExceptionRaiser));
        NotificationHelper.StopNotification(this);
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.LogError(e.Exception, nameof(TaskScheduler_UnobservedTaskException));
        NotificationHelper.StopNotification(this);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.LogError(e.ExceptionObject as Exception, nameof(CurrentDomain_UnhandledException));
        NotificationHelper.StopNotification(this);
    }

    public override void OnTerminate()
    {
        Logger.LogWarning($"Is terminating.");
        NotificationHelper.StopNotification(this);

        base.OnTerminate();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
