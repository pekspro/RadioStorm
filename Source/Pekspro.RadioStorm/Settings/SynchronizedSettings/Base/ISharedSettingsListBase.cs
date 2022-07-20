namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;

public interface ISharedSettingsListBase<T>
{
    int FileCount { get; }
    Dictionary<int, T> Items { get; set; }
    DateTime LatestChangedTime { get; }

    string GetFileName(int slotId);
    Task ReadLocalSettingsAsync();
    Task SynchronizeSettingsAsync(SynchronizeSettings synchronizeSettings, List<FileBaseProviderAndFiles> fileBaseProviderAndFiles);
}