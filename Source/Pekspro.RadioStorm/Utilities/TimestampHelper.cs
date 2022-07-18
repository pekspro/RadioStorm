namespace Pekspro.RadioStorm.Utilities;

public sealed class TimestampHelper
{
    private static long FirstTick = new DateTimeOffset(2015, 7, 10, 0, 0, 0, TimeSpan.FromSeconds(0)).Ticks;

    public static uint NowToInt()
    {
        return ToInt(DateTimeOffset.UtcNow);
    }

    public static uint ToInt(DateTimeOffset time)
    {
        if (time.Ticks < FirstTick)
        {
            return 0;
        }

        long tickDiff = time.Ticks - FirstTick;
        long secondDiff = tickDiff / (10 * 1000 * 1000);

        return (uint)secondDiff;
    }

    public static DateTimeOffset ToDateTime(uint timestamp)
    {
        long ticks = timestamp;
        ticks *= 10 * 1000 * 1000;
        ticks += FirstTick;

        return new DateTimeOffset(new DateTime(ticks));
    }
}
