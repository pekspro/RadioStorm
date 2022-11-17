using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Identity.Client;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

#nullable disable

namespace Pekspro.RadioStorm.MAUI;
[Activity(  Theme = "@style/Maui.SplashTheme", 
            MainLauncher = true, 
            ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize,
            LaunchMode = LaunchMode.SingleTop)]
public sealed class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        GraphHelper.AuthUIParent = this;

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

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
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
}
