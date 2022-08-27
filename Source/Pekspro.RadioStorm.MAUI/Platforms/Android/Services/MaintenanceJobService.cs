using Android.App;
using Android.App.Job;
using Android.Content;
using Pekspro.RadioStorm.CacheDatabase;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

[Service(Permission = "android.permission.BIND_JOB_SERVICE")]
public class MaintenanceJobService : JobService
{
    private ILogger? _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= Pekspro.RadioStorm.MAUI.Services.ServiceProvider.Current.GetRequiredService<ILogger<MediaPlayerService>>();
        }
    }

    private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    public override bool OnStartJob(JobParameters? jobParams)
    {
        Task.Run(async () =>
        {
            try
            {
                Logger.LogInformation("Starting maintenance.");

                var autoDeleteManager = Pekspro.RadioStorm.MAUI.Services.ServiceProvider.Current.GetRequiredService<IAutoDownloadDeleteManager>();
                autoDeleteManager.DeleteObseleteDownloads();

                var cachePrefetcher = Pekspro.RadioStorm.MAUI.Services.ServiceProvider.Current.GetRequiredService<ICachePrefetcher>();
                await cachePrefetcher.PrefetchAsync(CancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error starting maintenance: ({e.GetType()}) {e.Message}");
            }
            finally
            {
                Logger.LogInformation("Maintenance completed.");
            }
        });

        return true;
    }

    public override bool OnStopJob(JobParameters? jobParams)
    {
        Logger.LogInformation("Stopping maintenance.");

        CancellationTokenSource.Cancel();

        //true so we re-schedule the task
        return true;
    }

    public static JobInfo.Builder CreateJobBuilderUsingJobId(Context context, int jobId)
    {
        var javaClass = Java.Lang.Class.FromType(typeof(MaintenanceJobService));
        var componentName = new ComponentName(context, javaClass);
        return new JobInfo.Builder(jobId, componentName);
    }
}
