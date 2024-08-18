namespace Pekspro.RadioStorm.UI.Utilities;

public interface INotificationPermissionManager
{
    bool PermissionIsRequired { get; }

    bool HasPermission { get; }
    
    bool HasPermissionBeenDenied { get; }

    bool ShouldExplainWhyPermissionsIsNeeded { get; }

    void RequestPermission();
}
