namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.RecentHistory;

public interface IRecentPlayedManager : ISharedSettingsListBase<RecentPlayedItem>
{
    bool AddOrUpdate(bool isEpisode, int id);
    bool AddOrUpdate(bool isEpisode, int id, DateTimeOffset updateTime);
    void Clear();
    void Init(bool allowBackgroundSaving);
    void Remove(bool isEpisode, int id);
    Task SaveIfDirtyAsync();
}