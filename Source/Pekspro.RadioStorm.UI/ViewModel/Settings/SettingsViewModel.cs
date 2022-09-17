namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public sealed partial class SettingsViewModel : ObservableObject
{
    #region Private properties

    private CacheDatabaseManager CacheDatabaseManager { get; }

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
    }

    public SettingsViewModel(CacheDatabaseManager cacheDatabaseManager, ILocalSettings localSettings)
    {
        CacheDatabaseManager = cacheDatabaseManager;
        Settings = localSettings;
        ThemeTypes.Add(Strings.Settings_Theme_Default);
        ThemeTypes.Add(Strings.Settings_Theme_Light);
        ThemeTypes.Add(Strings.Settings_Theme_Dark);
    }

    #endregion

    #region Public properties

    public ILocalSettings Settings { get; }

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

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(CanClearCache))]
    private async void ClearCache()
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
}
