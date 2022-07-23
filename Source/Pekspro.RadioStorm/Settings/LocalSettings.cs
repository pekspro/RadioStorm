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

    public int Volume
    {
        get
        {
            int v = SettingsService.GetSafeValue(nameof(Volume), 100);

            return Math.Clamp(v, -1, 100);
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

    #endregion

    #region Methods

    private void NotifySettingChanged(string settingsName)
    {
        Messenger.Send(new SettingChangedMessage(settingsName));
    }

    #endregion
}
