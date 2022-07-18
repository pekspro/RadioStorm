namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite
{
    public record ChannelFavoriteChangedMessage(int? Id = null, bool IsAdded = false);

    public record ProgramFavoriteChangedMessage(int? Id = null, bool IsAdded = false);

    public record EpisodeSortOrderChangedMessage(int? Id = null, bool IsAdded = false);
}
