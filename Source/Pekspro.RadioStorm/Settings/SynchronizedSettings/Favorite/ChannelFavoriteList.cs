namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;

public class ChannelFavoriteList : FavoriteList, IChannelFavoriteList
{
    private const string FileNameSortOrder = "channelfavorites.dat";

    public ChannelFavoriteList
        (
            IMessenger messenger, 
            IDateTimeProvider dateTimeProvider,
            ILogger<ChannelFavoriteList> logger, 
            IOptions<StorageLocations> storageLocationOptions
        )
        : base(messenger, dateTimeProvider, logger, storageLocationOptions)
    {

    }

    public void Init(bool allowBackgroundSaving)
    {
        Init(FileNameSortOrder, allowBackgroundSaving, "Channels");
    }

    protected override void SendAllChangedMessage()
    {
        Messenger.Send(new ChannelFavoriteChangedMessage());
    }

    protected override void SendChangedMessage(int id, bool active)
    {
        Messenger.Send(new ChannelFavoriteChangedMessage(id, active));
    }
}
