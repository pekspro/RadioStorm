namespace Pekspro.RadioStorm.Settings.SynchronizedSettings;

public interface ISharedSettingsManager
{
    bool IsSynchronizing { get; }
    bool HasAnyRemoteSignedInProvider { get; }
    SynchronizingResult? LatestSynchronizingResult { get; }

    Task ForceSaveNowAsync();
    void Init(bool allowBackgroundSaving);
    Task ReadLocalSettingsAsync();
    void RegisterFilerProvider(IFileProvider fileProvider);
    Task SynchronizeSettingsAsync(SynchronizeSettings synchronizeSettings);
}