namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.ListenState;

public sealed record ListenStateChangedMessage(int? EpisodeId, bool IsListened);

