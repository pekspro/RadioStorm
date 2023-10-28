using Android.Content;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

public class ReviewLauncherAndroid : IReviewLauncher
{
    #region Private properties

    private ILogger Logger { get; }

    #endregion

    #region Constructor

    public ReviewLauncherAndroid(ILogger<ReviewLauncherAndroid> logger)
    {
        Logger = logger;
    }

    #endregion


    #region Methods

    public Task LaunchReviewAsync()
    {
        OpenStoreReviewPage("com.pekspro.radiostorm");

        return Task.CompletedTask;
    }

    #endregion

    Intent GetRateIntent(string url)
    {
        var intent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url));

        intent.AddFlags(ActivityFlags.NoHistory);
        intent.AddFlags(ActivityFlags.MultipleTask);
        intent.AddFlags(ActivityFlags.NewDocument);
        intent.SetFlags(ActivityFlags.ClearTop);
        intent.SetFlags(ActivityFlags.NewTask);

        return intent;
    }

    public void OpenStoreReviewPage(string appId)
    {
        var url = $"market://details?id={appId}";
        try
        {
            var intent = GetRateIntent(url);
            ((MainApplication)MauiApplication.Current).ApplicationContext!.StartActivity(intent);
            return;
        }
        catch (Exception ex)
        {
            Logger.LogInformation(ex, "Unable to launch app store: " + ex.Message);
        }

        url = $"https://play.google.com/store/apps/details?id={appId}";
        try
        {
            var intent = GetRateIntent(url);
            ((MainApplication)MauiApplication.Current).ApplicationContext!.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Logger.LogInformation(ex, "Unable to launch browser: " + ex.Message);
        }
    }
}
