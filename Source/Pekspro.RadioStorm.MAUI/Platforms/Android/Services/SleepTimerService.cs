using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Pekspro.RadioStorm.UI.Resources;
using static Android.App.Notification;
using Binder = Android.OS.Binder;

#nullable disable

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

[Service(Exported = true, ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeNone)]
[IntentFilter(new[] { ActionStopSleepTimer, ActionIncrease5Minutes })]
public sealed class SleepTimerService : Service
{
    // Actions
    public const string ActionStopSleepTimer = "com.pekspro.radiostorm.action.STOP_SLEEP_TIMER";
    public const string ActionIncrease5Minutes = "com.pekspro.radiostorm.action.STOP_SLEEP_TIMER_AND_PAUSE";

    public const string CHANNEL_ID = "SleepTimer";

    private IBinder mBinder;

    private ILogger _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= MAUI.Services.ServiceProvider.Current.GetRequiredService<ILogger<SleepTimerService>>();
        }
    }
    
    private IAudioManager AudioManager
    {
        get
        {
            return MAUI.Services.ServiceProvider.Current.GetRequiredService<IAudioManager>();
        }
    }

    public override void OnCreate()
    {
        Logger.LogInformation("{serviceName} starts", nameof(SleepTimerService));
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var name = "Sleep Timer Service Notification";
            var description = "Status of sleep timer.";
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Low)
            {
                Description = description
            };

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        UpdateNotification(AudioManager.TimeLeftToSleepActivation);

        base.OnCreate();
    }

    public override void OnDestroy()
    {
        Logger.LogInformation("{serviceName} stops", nameof(SleepTimerService));

        base.OnDestroy();
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        if (intent.Action == ActionStopSleepTimer)
        {
            AudioManager.StopSleepTimer();
        }
        else if (intent.Action == ActionIncrease5Minutes)
        {
            AudioManager.IncreaseSleepTimer();
        }
            
        return base.OnStartCommand(intent, flags, startId);
    }

    private TimeSpan PreviousTimeLeftToSleepActivation = TimeSpan.FromSeconds(-1);

    public void UpdateNotification(TimeSpan timeLeftToSleepActivation)
    {
        if (timeLeftToSleepActivation != PreviousTimeLeftToSleepActivation)
        {
            (string shortDescription, string longDescription) = CreateNotificationContextText(timeLeftToSleepActivation);
            
            Notification notification = CreateNotification(this, shortDescription, longDescription);
            StartForeground(1, notification);
        }
    }

    private (string shortDescription, string longDescription) CreateNotificationContextText(TimeSpan timeLeftToSleepActivation)
    {
        return 
            (
                string.Format(Strings.SleepTimer_Notification_Title, timeLeftToSleepActivation.Minutes, timeLeftToSleepActivation.Seconds),
                Strings.SleepTimer_Notification_Description
            );
    }

    internal static Notification.Action GenerateActionCompat(Context context, int icon, string title, string intentAction)
    {
        Intent intent = new Intent(context, typeof(SleepTimerService));
        intent.SetAction(intentAction);

        PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
        flags |= PendingIntentFlags.Mutable;

        PendingIntent pendingIntent = PendingIntent.GetService(context, 1, intent, flags);

        return new Notification.Action.Builder(icon, title, pendingIntent).Build();
    }

    private Notification CreateNotification(Context context, string shortDescription, string longDescription)
    {
        var cancelAction = GenerateActionCompat(context, Resource.Drawable.ic_notification_skip_next, Strings.SleepTimer_Notification_Action_Cancel, ActionStopSleepTimer);
        var increase5minutesAction = GenerateActionCompat(context, Resource.Drawable.ic_notification_pause, Strings.SleepTimer_Increase_5min, ActionIncrease5Minutes);

        var notificationBuilder = new Builder(this, CHANNEL_ID)
            .SetContentTitle(shortDescription)
            .SetContentText(longDescription)
            .SetSmallIcon(Resource.Drawable.ic_statusbar_download)
            .AddAction(cancelAction)
            .AddAction(increase5minutesAction);

        return notificationBuilder.Build();
    }

    public override IBinder OnBind(Intent intent)
    {
        if (mBinder is null)
        {
            mBinder = new SleepTimerServiceBinder(this);
        }
        
        return mBinder;
    }
}

public sealed class SleepTimerServiceBinder : Binder
{
    private readonly SleepTimerService service;

    public SleepTimerServiceBinder(SleepTimerService service)
    {
        this.service = service;
    }

    public SleepTimerService GetSleepTimerService()
    {
        return service;
    }
}
