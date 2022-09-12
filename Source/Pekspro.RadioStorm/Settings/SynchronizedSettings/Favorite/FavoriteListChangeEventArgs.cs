namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Favorite;

public sealed record ChannelFavoriteChangedMessage(int? Id = null, bool IsAdded = false);

public sealed record ProgramFavoriteChangedMessage(int? Id = null, bool IsAdded = false);

public sealed record EpisodeSortOrderChangedMessage(int? Id = null, bool IsAdded = false);
