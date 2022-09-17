namespace Pekspro.RadioStorm.Audio.Message;

public sealed record SleepStateChanged(bool IsSleepModeActivated, TimeSpan TimeLeftToSleepActivation);
