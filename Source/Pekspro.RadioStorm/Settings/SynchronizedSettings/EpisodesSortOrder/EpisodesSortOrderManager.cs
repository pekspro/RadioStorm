namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.EpisodesSortOrder
{
    public class EpisodesSortOrderManager : FavoriteList, IEpisodesSortOrderManager
    {
        private const string FileNameSortOrder = "episodessortorder.dat";

        public EpisodesSortOrderManager
            (
                IMessenger messenger, 
                IDateTimeProvider dateTimeProvider,
                ILogger<EpisodesSortOrderManager> logger, 
                IOptions<StorageLocations> storageLocationOptions
            )
            : base(messenger, dateTimeProvider, logger, storageLocationOptions)
        {

        }

        public void Init(bool allowBackgroundSaving)
        {
            Init(FileNameSortOrder, allowBackgroundSaving, "EpisodesSort");
        }

        protected override void SendAllChangedMessage()
        {
            Messenger.Send(new EpisodeSortOrderChangedMessage());
        }

        protected override void SendChangedMessage(int id, bool active)
        {
            Messenger.Send(new EpisodeSortOrderChangedMessage(id, active));
        }
    }
}
