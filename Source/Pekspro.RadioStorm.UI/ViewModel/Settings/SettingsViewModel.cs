namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public sealed partial class SettingsViewModel : ObservableObject
{
    #region Private properties

    private CacheDatabaseManager CacheDatabaseManager { get; }

    private IVersionProvider VersionProvider { get; }

    private IUriLauncher UriLauncher { get; }

    [ObservableProperty]
    private bool _CanClearCache = true;

    [ObservableProperty]
    private bool _IsCacheCleared = false;

    #endregion

    #region Constructor

    public SettingsViewModel()
    {
        CacheDatabaseManager = null!;
        Settings = null!;
        NotificationPermissionManager = null!;
        VersionProvider = null!;
        UriLauncher = null!;
    }

    public SettingsViewModel(CacheDatabaseManager cacheDatabaseManager, ILocalSettings localSettings, INotificationPermissionManager notificationPermissionManager, 
        IMessenger messenger, IVersionProvider versionProvider, IUriLauncher uriLauncher)
    {
        CacheDatabaseManager = cacheDatabaseManager;
        Settings = localSettings;
        NotificationPermissionManager = notificationPermissionManager;
        VersionProvider = versionProvider;
        UriLauncher = uriLauncher;
        ThemeTypes.Add(Strings.Settings_Theme_Default);
        ThemeTypes.Add(Strings.Settings_Theme_Light);
        ThemeTypes.Add(Strings.Settings_Theme_Dark);

        messenger?.Register<NotificationPermissionChangedMessage>(this, (r, m) =>
        {
            RefreshNotificationSettings();
        });
    }

    #endregion

    #region Public properties

    public ILocalSettings Settings { get; }
    public INotificationPermissionManager NotificationPermissionManager { get; }

    [ObservableProperty]
    private bool _IsCacheClearing = false;

    public List<string> ThemeTypes { get; } = new List<string>();

    public int ThemeIndex
    {
        get
        {
            return (int) Settings.Theme;
        }
        set
        {
            Settings.Theme = value switch
            {
                1 => ThemeType.Light,
                2 => ThemeType.Dark,
                _ => ThemeType.Auto
            };
        }
    }

    public string VersionString => $"RadioStorm {VersionProvider.ApplicationVersion}";

    public string PureVersionString => VersionProvider.ApplicationVersion.ToString();
   
    public bool NotificationPermissionIsRequired => NotificationPermissionManager.PermissionIsRequired;

    public bool HasNotificationPermission => NotificationPermissionManager.HasPermission;

    public bool HasNotificationPermissionBeenDenied => !HasNotificationPermission && NotificationPermissionManager.HasPermissionBeenDenied;

    public bool NotificationPermissionsCanBeRequested => !HasNotificationPermission && !HasNotificationPermissionBeenDenied;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task Email()
    {
        await UriLauncher.LaunchAsync(new Uri($"mailto:{Strings.General_Pekspro_EmailAddress}"));
    }

    [RelayCommand]
    private async Task OpenWebPage()
    {
        await UriLauncher.LaunchAsync(new Uri(Strings.General_Pekspro_Url));
    }

    [RelayCommand]
    private void RequestNotificationPermission()
    {
        NotificationPermissionManager.RequestPermission();
    }

    [RelayCommand(CanExecute = nameof(CanClearCache))]
    private async Task ClearCache()
    {
        CanClearCache = false;
        IsCacheCleared = false;
        IsCacheClearing = true;
        ClearCacheCommand.NotifyCanExecuteChanged();

        await CacheDatabaseManager.ClearAsync();

        IsCacheCleared = true;
        IsCacheClearing = false;
    }

    #endregion

    #region Methods

    public void RefreshNotificationSettings()
    {
        OnPropertyChanged(nameof(HasNotificationPermission));
        OnPropertyChanged(nameof(HasNotificationPermissionBeenDenied));
        OnPropertyChanged(nameof(NotificationPermissionsCanBeRequested));
    }

    #endregion

}
