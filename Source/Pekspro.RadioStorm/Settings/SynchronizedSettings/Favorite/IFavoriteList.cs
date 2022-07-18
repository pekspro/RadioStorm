using static Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite.FavoriteList;

namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite
{
    public interface IFavoriteList : ISharedSettingsListBase<FavoriteItem>
    {
        // void Init(string filename, bool allowBackgroundSaving, string logName);
        bool IsFavorite(int id);
        Task SaveIfDirtyAsync();
        bool SetFavorite(int id, bool active);
    }
}