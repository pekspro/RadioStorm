namespace Pekspro.RadioStorm.Utilities;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateTime LocalNow { get; }

    DateTime SwedishNow { get; }

    DateTimeOffset OffsetNow { get; }
}
