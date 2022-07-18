namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite
{
    public interface IProgramFavoriteList : IFavoriteList
    {
        void Init(bool allowBackgroundSaving);
    }
}