namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.Base;

public sealed class SharedSettingsState
{
    public bool IsSynchronizing { get; set; }

    public DateTime? LatestSynchronizingTime { get; set; }
}
