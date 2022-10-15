namespace Pekspro.RadioStorm.Settings;

public sealed class LocalSettings : ILocalSettings
{
    #region Private properties

    private ISettingsService SettingsService { get; }
    
    private IMessenger Messenger { get; }
    
    public static int LaunchCountBeforeAskForReview => 20;

    #endregion

    #region Constructor

    public LocalSettings(ISettingsService settingsService, IMessenger messenger)
    {
        SettingsService = settingsService;
        Messenger = messenger;
    }

    #endregion

    #region Properties

    public double Volume
    {
        get
        {
            double v = SettingsService.GetSafeValue(nameof(Volume), 1.0);

            return Math.Clamp(v, 0, 1);
        }
        set
        {
            SettingsService.SetValue(nameof(Volume), value);

            NotifySettingChanged(nameof(Volume));
        }
    }

    public int AutoRemoveListenedDownloadedFilesDayDelay
    {
        get
        {
            int v = SettingsService.GetSafeValue(nameof(AutoRemoveListenedDownloadedFilesDayDelay), 7);

            return Math.Clamp(v, -1, 365);
        }
        set
        {
            SettingsService.SetValue(nameof(AutoRemoveListenedDownloadedFilesDayDelay), value);

            NotifySettingChanged(nameof(AutoRemoveListenedDownloadedFilesDayDelay));
        }
    }

    public bool UseLiveTile
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(UseLiveTile), true);
        }
        set
        {
            SettingsService.SetValue(nameof(UseLiveTile), value);

            NotifySettingChanged(nameof(UseLiveTile));
        }
    }

    public int LaunchCount
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(LaunchCount), 0);
        }
        set
        {
            SettingsService.SetValue(nameof(LaunchCount), value);

            NotifySettingChanged(nameof(LaunchCount));
        }
    }

    public int LastSetSleepTimerIntervall
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(LastSetSleepTimerIntervall), 300);
        }
        set
        {
            SettingsService.SetValue(nameof(LastSetSleepTimerIntervall), value);

            NotifySettingChanged(nameof(LastSetSleepTimerIntervall));
        }
    }
    
    public bool MayWantToReview
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(MayWantToReview), true);
        }
        set
        {
            SettingsService.SetValue(nameof(MayWantToReview), value);

            NotifySettingChanged(nameof(MayWantToReview));
        }
    }

    public bool PreferStreamsWithMusic
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(PreferStreamsWithMusic), true);
        }
        set
        {
            SettingsService.SetValue(nameof(PreferStreamsWithMusic), value);

            NotifySettingChanged(nameof(PreferStreamsWithMusic));
        }
    }

    public bool WriteLogsToFile
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(WriteLogsToFile), false);
        }
        set
        {
            SettingsService.SetValue(nameof(WriteLogsToFile), value);

            NotifySettingChanged(nameof(WriteLogsToFile));
        }
    }

    public bool FavoriteAlbumMode
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(FavoriteAlbumMode), true);
        }
        set
        {
            SettingsService.SetValue(nameof(FavoriteAlbumMode), value);

            NotifySettingChanged(nameof(FavoriteAlbumMode));
        }
    }

    public ThemeType Theme
    {
        get
        {
            return (ThemeType) SettingsService.GetSafeValue(nameof(Theme), (int) ThemeType.Dark);
        }
        set
        {
            SettingsService.SetValue(nameof(Theme), (int) value);
            
            NotifySettingChanged(nameof(Theme));
        }
    }

    #endregion

    #region Methods

    private void NotifySettingChanged(string settingsName)
    {
        Messenger.Send(new SettingChangedMessage(settingsName));
    }

    #endregion
}
