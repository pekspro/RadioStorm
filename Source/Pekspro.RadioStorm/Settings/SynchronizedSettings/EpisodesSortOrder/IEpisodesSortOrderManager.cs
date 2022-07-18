namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.EpisodesSortOrder
{
    public interface IEpisodesSortOrderManager : IFavoriteList
    {
        void Init(bool allowBackgroundSaving);
    }
}