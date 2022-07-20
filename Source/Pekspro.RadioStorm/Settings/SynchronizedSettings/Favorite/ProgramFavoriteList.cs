namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;

public class ProgramFavoriteList : FavoriteList, IProgramFavoriteList
{
    private const string FileNameSortOrder = "programfavorites.dat";

    public ProgramFavoriteList
        (
            IMessenger messenger,
            IDateTimeProvider dateTimeProvider,
            ILogger<ProgramFavoriteList> logger, 
            IOptions<StorageLocations> storageLocationOptions
        )
        : base(messenger, dateTimeProvider, logger, storageLocationOptions)
    {

    }

    public void Init(bool allowBackgroundSaving)
    {
        Init(FileNameSortOrder, allowBackgroundSaving, "Programs");
    }

    protected override void SendAllChangedMessage()
    {
        Messenger.Send(new ProgramFavoriteChangedMessage());
    }

    protected override void SendChangedMessage(int id, bool active)
    {
        Messenger.Send(new ProgramFavoriteChangedMessage(id, active));
    }
}
