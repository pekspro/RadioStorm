using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

#nullable disable

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

public class NotificationPermissionManagerAndroid : INotificationPermissionManager
{
    private Context Context => Platform.AppContext;

    private global::Android.App.Activity Activity => Platform.CurrentActivity;

    public bool PermissionIsRequired => OperatingSystem.IsAndroidVersionAtLeast(33);

    public bool HasPermission
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                Permission permission = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.PostNotifications);

                return permission == Permission.Granted;
            }
            else
            {
                return true;
            }
        }
    }

    public bool HasPermissionBeenDenied => ((MainActivity)Activity).HasNotificationPermissionBeenDenied;

    public bool ShouldExplainWhyPermissionsIsNeeded
    {
        get
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                return ActivityCompat.ShouldShowRequestPermissionRationale(Activity, Manifest.Permission.PostNotifications);
            }
            else
            {
                return true;
            }
        }
    }

    public void RequestPermission()
    {
        if (HasPermissionBeenDenied || HasPermission)
        {
            ((MainActivity)Activity).OpenAppSettings();
        }
        else
        {
            ((MainActivity)Activity).RequestNotificationPermission();
        }
    }
}
