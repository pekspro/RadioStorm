using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Identity.Client;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

#nullable disable

namespace Pekspro.RadioStorm.MAUI;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    internal static MainActivity instance;
    public MediaPlayerServiceBinder binder;
    MediaPlayerServiceConnection mediaPlayerServiceConnection;
    private Intent mediaPlayerServiceIntent;

    public event StatusChangedEventHandler StatusChanged;


    protected override async void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        instance = this;
        NotificationHelper.CreateNotificationChannel(ApplicationContext);
        if (mediaPlayerServiceConnection is null)
        {
            InitilizeMedia();
        }

        GraphHelper.AuthUIParent = this;

        // await MauiProgram.SetupAsync();

        #region Scheduler

        var jobBuilder = MaintenanceJobService.CreateJobBuilderUsingJobId(this, 152);

        jobBuilder
        //    //This means each 20 mins                
            .SetPeriodic(24 * 60 * 60 * 1000)
        //    //.SetPeriodic(20 * 1000)
        //    //Persists over phone restarts
            .SetPersisted(true)

            .SetRequiredNetworkType(NetworkType.Unmetered)
            .SetRequiresDeviceIdle(true)
            .SetRequiresCharging(true)
        //    //.SetRequiresDeviceIdle(false)

        //    //If Fails re-try each 2 mins
        //    //.SetBackoffCriteria(120 * 1000, BackoffPolicy.Linear)
            ;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
        {
            jobBuilder.SetPrefetch(true);
        }

        var jobInfo = jobBuilder.Build();
        

        var jobScheduler = (JobScheduler) GetSystemService(JobSchedulerService);
        jobScheduler.Cancel(152);
        var scheduleResult = jobScheduler.Schedule(jobInfo);

        if (JobScheduler.ResultSuccess == scheduleResult)
        {
            //If OK maybe show a msg
        }
        else
        {
            //If Failed do something
        }

        #endregion
        // Platform.Init(this, savedInstanceState);
    }

    private void InitilizeMedia()
    {
        mediaPlayerServiceIntent = new Intent(ApplicationContext, typeof(MediaPlayerService));
        mediaPlayerServiceConnection = new MediaPlayerServiceConnection(this);
        BindService(mediaPlayerServiceIntent, mediaPlayerServiceConnection, Bind.AutoCreate);
    }

    /*
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
    */

    // <OnActivityResultSnippet>
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        AuthenticationContinuationHelper
            .SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
    }
    // </OnActivityResultSnippet>



    class MediaPlayerServiceConnection : Java.Lang.Object, IServiceConnection
    {
        readonly MainActivity instance;

        public MediaPlayerServiceConnection(MainActivity mediaPlayer)
        {
            this.instance = mediaPlayer;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            if (service is MediaPlayerServiceBinder mediaPlayerServiceBinder)
            {
                var binder = (MediaPlayerServiceBinder)service;
                instance.binder = binder;

                binder.GetMediaPlayerService().StatusChanged += (object sender, EventArgs e) => { instance.StatusChanged?.Invoke(sender, e); };
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
        }
    }
}
