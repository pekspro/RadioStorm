namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite
{
    public interface IChannelFavoriteList : IFavoriteList
    {
        void Init(bool allowBackgroundSaving);
    }
}