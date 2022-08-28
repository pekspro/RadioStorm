using Android.App;
using Android.Runtime;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

[assembly: UsesPermission(Android.Manifest.Permission.AccessNetworkState)]

namespace Pekspro.RadioStorm.MAUI;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
        
        // NotificationHelper.StopNotification(this);
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
