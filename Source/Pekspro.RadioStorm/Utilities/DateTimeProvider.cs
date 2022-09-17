namespace Pekspro.RadioStorm.Utilities;

public sealed class DateTimeProvider : IDateTimeProvider
{
    private TimeZoneInfo? _SwedishTimeZone;

    public TimeZoneInfo SwedishTimeZone => _SwedishTimeZone ??= TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public DateTime SwedishNow
    {
        get
        {
            DateTime now = DateTime.UtcNow;

            DateTime swedishNow = TimeZoneInfo.ConvertTimeFromUtc(now, SwedishTimeZone);

            return swedishNow;
        }
    }

    public DateTime UtcNow => DateTime.UtcNow;
    
    public DateTime LocalNow => DateTime.Now;

    public DateTimeOffset OffsetNow => DateTimeOffset.Now;
}
