namespace Pekspro.RadioStorm.Settings.SynchronizedSettings;

public sealed record SynchronizingResult ( DateTimeOffset SynchronizingTime, bool Successful, bool HadInternetAccess, IReadOnlySet<string> FailedProviders );
