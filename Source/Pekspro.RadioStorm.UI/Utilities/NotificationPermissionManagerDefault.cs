namespace Pekspro.RadioStorm.UI.Utilities;

public class NotificationPermissionManagerDefault : INotificationPermissionManager
{
    public bool HasPermission => true;

    public bool HasPermissionBeenDenied => false;

    public bool PermissionIsRequired => false;

    public bool ShouldExplainWhyPermissionsIsNeeded => false;

    public void RequestPermission() { }
}
