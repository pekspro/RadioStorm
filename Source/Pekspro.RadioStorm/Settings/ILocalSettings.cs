namespace Pekspro.RadioStorm.Settings;

public interface ILocalSettings
{
    double Volume { get; set; }
    int AutoRemoveListenedDownloadedFilesDayDelay { get; set; }
    int LaunchCount { get; set; }
    bool MayWantToReview { get; set; }
    bool PreferStreamsWithMusic { get; set; }
    bool UseLiveTile { get; set; }
    bool ShowDebugSettings { get; set; }
    bool WriteLogsToFile { get; set; }
    ThemeType Theme { get; set; }
}