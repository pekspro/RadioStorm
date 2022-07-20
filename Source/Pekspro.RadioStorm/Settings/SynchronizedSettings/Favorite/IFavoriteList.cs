using static Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite.FavoriteList;

namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;

public interface IFavoriteList : ISharedSettingsListBase<FavoriteItem>
{
    bool IsFavorite(int id);
    Task SaveIfDirtyAsync();
    bool SetFavorite(int id, bool active);
}