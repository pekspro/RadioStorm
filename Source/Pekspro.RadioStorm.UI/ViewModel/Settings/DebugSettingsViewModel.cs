namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public partial class DebugSettingsViewModel : ObservableObject
{
    #region Private properties

    public ILocalSettings Settings { get; }
    
    #endregion

    #region Constructor

    public DebugSettingsViewModel()
    {
        Settings = null!;
    }

    public DebugSettingsViewModel(ILocalSettings localSettings)
    {
        Settings = localSettings;
    }

    #endregion

    #region Public properties

    public bool ShowDebugSettings
    {
        get
        {
            return Settings.ShowDebugSettings;
        }
        set
        {
            if (Settings.ShowDebugSettings != value)
            {
                Settings.ShowDebugSettings = value;
                OnPropertyChanged(nameof(ShowDebugSettings));
            }
        }
    }

    #endregion
}
