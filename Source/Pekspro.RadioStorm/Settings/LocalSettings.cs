namespace Pekspro.RadioStorm.Settings;

public sealed class LocalSettings : ILocalSettings
{
    public ISettingsService SettingsService { get; }
    public IMessenger Messenger { get; }

    public LocalSettings(ISettingsService settingsService, IMessenger messenger)
    {
        SettingsService = settingsService;
        Messenger = messenger;
    }

    public static int LaunchCountBeforeAskForReview => 20;

    #region Volume property

    public int Volume
    {
        get
        {
            int v = SettingsService.GetSafeValue(nameof(Volume), 100);

            if (v < -1)
            {
                return -1;
            }

            if (v > 100)
            {
                return 100;
            }

            return v;
        }
        set
        {
            SettingsService.SetValue(nameof(Volume), value);

            NotifySettingChanged(nameof(Volume));
        }
    }

    #endregion

    #region AutoRemoveListenedDownloadedFilesDayDelay property

    public int AutoRemoveListenedDownloadedFilesDayDelay
    {
        get
        {
            int v = SettingsService.GetSafeValue(nameof(AutoRemoveListenedDownloadedFilesDayDelay), 7);

            if (v < -1)
            {
                return -1;
            }

            if (v > 365)
            {
                return 365;
            }

            return v;
        }
        set
        {
            SettingsService.SetValue(nameof(AutoRemoveListenedDownloadedFilesDayDelay), value);

            NotifySettingChanged(nameof(AutoRemoveListenedDownloadedFilesDayDelay));
        }
    }

    #endregion

    #region ShowToastWhenBackgroundDownloadFinished property

    public bool ShowToastWhenBackgroundDownloadFinished
    {
        get
        {
            return SettingsService.GetSafeValue(nameof(ShowToastWhenBackgroundDownloadFinished), true);
        }
        set
        {
            SettingsService.SetValue(nameof(ShowToastWhenBackgroundDownloadFinished), value);

            NotifySettingChanged(nameof(ShowToastWhenBackgroundDownloadFinished));
        }
    }

    #endregion

    #region UseLiveTile property

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

    #endregion

    #region LaunchCount property

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

    #endregion

    #region MayWantToReview property

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

    #endregion

    #region PreferStreamsWithMusic property

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

    private void NotifySettingChanged(string settingsName)
    {
        Messenger.Send(new SettingChanged(settingsName));
    }
}
