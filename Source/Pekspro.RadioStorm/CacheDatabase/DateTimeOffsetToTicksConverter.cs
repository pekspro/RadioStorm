namespace Pekspro.RadioStorm.CacheDatabase;

public class DateTimeOffsetToTicksConverter : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToTicksConverter()
        : base(
            v => v.UtcTicks,
            v => new DateTimeOffset(new DateTime(v, DateTimeKind.Utc)).ToLocalTime()
        )
    {
    }    
}
