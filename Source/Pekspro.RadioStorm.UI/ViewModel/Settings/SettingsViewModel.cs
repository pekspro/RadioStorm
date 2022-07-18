namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class SettingsViewModel : ObservableObject
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
    }

    #endregion

    #region Public properties

    public ILocalSettings Settings { get; }

    [ObservableProperty]
    private bool _IsCacheClearing = false;

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
