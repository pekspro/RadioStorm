using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Services;
using static Microsoft.Maui.ApplicationModel.Platform;
using Intent = Android.Content.Intent;

[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]

namespace Pekspro.RadioStorm.MAUI;

[Application]
public class MainApplication : MauiApplication
{
    class DownloadServiceConnection : Java.Lang.Object, IServiceConnection
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

    private DownloadServiceConnection DownloadServiceConnectionObject { get; }
    private DownloadServiceBinder? DownloadServiceBinder;

    private ConnectivityManager? connectivityManager;

    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;

        NotificationHelper.StopNotification(this);

        DownloadServiceConnectionObject = new DownloadServiceConnection(this);
    }

    public override void OnCreate()
    {
        base.OnCreate();

        connectivityManager = (ConnectivityManager?) GetSystemService(ConnectivityService);
        connectivityManager?.RegisterDefaultNetworkCallback(new ConnectivityMonitor());

        IMessenger? messenger = Services.GetService<IMessenger>();

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
        }
    }

    protected void UpdateDownloadServiceExpectedStatus()
    {
        IMainThreadRunner? mainThreadRunner = Services.GetService<IMainThreadRunner>();
        IDownloadManager? downloadManager = Services.GetService<IDownloadManager>();

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

    protected void StartDownloadService()
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
            Logger.LogError("Failed to start download service start.");
        }
    }

    protected void StopDownloadService()
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

    private ILogger? _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= Services.GetRequiredService<ILogger<App>>();
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
