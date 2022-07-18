using Android.App;
using Android.App.Job;
using Android.Content;
using Pekspro.RadioStorm.CacheDatabase;

namespace Microsoft.NetConf2021.Maui.Platforms.Android.Services;

[Service(Permission = "android.permission.BIND_JOB_SERVICE")]
public class MaintenanceJobService : JobService
{
    private static string _LogFileName = null;

    private static string LogFileName
    {
        get
        {
            return _LogFileName ??= System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "maintenance.log");
        }
    }

    static void WriteLog(string text)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine(text);
            // Write to log with timestamp
            File.AppendAllText(LogFileName, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {text}\r\n");
        }
        catch(Exception )
        {            

        }
    }

    public static string GetLog()
    {
        try
        {
            return File.ReadAllText(LogFileName);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static void ClearLog()
    {
        try
        {
            File.Delete(LogFileName);
        }
        catch (Exception)
        {
            
        }
    }

    private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    public override bool OnStartJob(JobParameters jobParams)
    {
        Task.Run(async () =>
        {
            try
            {
                WriteLog("Starting maintenance.");

                var autoDeleteManager = Pekspro.RadioStorm.MAUI.Services.ServiceProvider.Current.GetRequiredService<IAutoDownloadDeleteManager>();
                autoDeleteManager.DeleteObseleteDownloads();

                var cachePrefetcher = Pekspro.RadioStorm.MAUI.Services.ServiceProvider.Current.GetRequiredService<ICachePrefetcher>();
                await cachePrefetcher.PrefetchAsync(CancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                WriteLog($"Error starting maintenance: ({e.GetType()}) {e.Message}");
            }
            finally
            {
                WriteLog("Maintenance completed.");
            }
        });

        return true;
    }

    public override bool OnStopJob(JobParameters jobParams)
    {
        WriteLog("Stopping maintenance.");

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
