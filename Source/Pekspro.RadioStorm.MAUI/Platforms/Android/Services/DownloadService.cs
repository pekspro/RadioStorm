// #define USE_CONNECTION_ALIVE_CHECKER
// #define USE_WIFI_LOCK

using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Pekspro.RadioStorm.UI.Resources;
using static Android.App.Notification;
using Binder = Android.OS.Binder;

#nullable disable

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

[Service(Exported = false, ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public sealed class DownloadService : Service
{
    public const string CHANNEL_ID = "DownloadServiceChannel";

    private IBinder mBinder;

    private WifiManager wifiManager;
    private WifiManager.WifiLock wifiLock;

    private PowerManager powerManager;
    private PowerManager.WakeLock mWakeLock;

    private ILogger _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= MAUI.Services.ServiceProvider.Current.GetRequiredService<ILogger<DownloadService>>();
        }
    }


    public override void OnCreate()
    {
        powerManager = (PowerManager) GetSystemService(PowerService);
        wifiManager = (WifiManager) GetSystemService(WifiService);

        StartWakeLock();
        AquireWifiLock();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var name = "Download Service Notification";
            var description = "Status of downloads.";
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Low)
            {
                Description = description
            };

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        UpdateNotification();

        base.OnCreate();
    }

    private string PreviousContextText;

    public void UpdateNotification()
    {
        (string shortDescription, string longDescription) = CreateNotificationContextText();
        
        if (PreviousContextText != longDescription)
        {
            PreviousContextText = longDescription;
            
            Notification notification = CreateNotification(shortDescription, PreviousContextText);
            StartForeground(1, notification);
        }
    }

    private (string shortDescription, string longDescription) CreateNotificationContextText()
    {
        string shortDescription = Strings.DownloadsNotification_Description_SingleLeft;
        
        StringBuilder sb = new StringBuilder();
        
        IDownloadManager downloadManager = MAUI.Services.ServiceProvider.GetService<IDownloadManager>();

        if (downloadManager is not null)
        {
            var userDownloads = downloadManager.GetActiveUserDownloads();

            if (userDownloads.Count > 1)
            {
                shortDescription = string.Format(Strings.DownloadsNotification_Description_MultipleLeft, userDownloads.Count);
            }

            foreach (var userDownload in userDownloads)
            {
                sb.Append($"\u2022 {userDownload.ProgramName} - {userDownload.EpisodeTitle}");

                if (userDownload.Status is DownloadDataStatus.Downloading)
                {
                    double ratio = 0;
                    if (userDownload.BytesToDownload > 0)
                    {
                        ratio = userDownload.BytesDownloaded / (double)userDownload.BytesToDownload;

                        sb.Append($" ({(int)(ratio * 100 + 0.5)} %)");
                    }
                }
                else
                {
                    sb.Append($" ({Strings.DownloadsNotification_Description_InQueue})");
                }
                
                sb.AppendLine();
            }
        }

        return (shortDescription, sb.ToString());
    }
    
    private Notification CreateNotification(string shortDescription, string longDescription)
    {
        // TODO: Add when this is released: https://github.com/dotnet/maui/issues/9090
        // var openAppIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), PendingIntentFlags.UpdateCurrent);

        Notification notification = new Notification.Builder(this, CHANNEL_ID)
            .SetContentTitle(Strings.DownloadsNotification_Title)
            .SetContentText(shortDescription)
            .SetSmallIcon(Resource.Drawable.ic_statusbar_download)
            .SetStyle(new BigTextStyle().BigText(longDescription))
            //.SetContentIntent(openAppIntent);
            .Build();

        return notification;
    }

    private void StartWakeLock()
    {
        if (mWakeLock is not null)
        {
            Logger.LogInformation("Power wake lock already aquired.");
            return;
        }
        
        Logger.LogInformation("Aquire power wake lock.");
        
        mWakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "DownloadService::PowerWakeLock");
        mWakeLock.Acquire();
    }

    private void StopWakeLock()
    {
        if (mWakeLock is null)
        {
            Logger.LogInformation("Power wake lock not aquired.");
            return;
        }

        Logger.LogInformation("Releasing power wake lock.");
        
        mWakeLock.Release();
        mWakeLock = null;
    }

    private void AquireWifiLock()
    {
        if (wifiLock is not null)
        {
            Logger.LogInformation("Wifi lock already aquired.");
            return;
        }

        Logger.LogInformation("Aquire wifi lock.");

        wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "radiostorm_download_service_wifi_lock");

        wifiLock.Acquire();
    }

    private void ReleaseWifiLock()
    {
        if (wifiLock is null)
        {
            Logger.LogInformation("No wifi lock aquired.");
            return;
        }

        Logger.LogInformation("Releasing wifi lock.");

        wifiLock.Release();
        wifiLock = null;
    }

    public override void OnDestroy()
    {
        StopWakeLock();
        ReleaseWifiLock();
        
        base.OnDestroy();
    }

    public override IBinder OnBind(Intent intent)
    {
        if (mBinder is null)
        {
            mBinder = new DownloadServiceBinder(this);
        }
        
        return mBinder;
    }
}

public sealed class DownloadServiceBinder : Binder
{
    private readonly DownloadService service;

    public DownloadServiceBinder(DownloadService service)
    {
        this.service = service;
    }

    public DownloadService GetDownloadService()
    {
        return service;
    }
}
