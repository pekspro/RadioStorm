namespace Pekspro.RadioStorm.CacheDatabase;

public class CacheTime
{
    public static TimeSpan ChannelDataCacheTime = new TimeSpan(7, 0, 0, 0);
    public static TimeSpan ChannelStatusCacheTime = new TimeSpan(0, 0, 0, 15);
    public static TimeSpan ChannelSongListCacheTime = new TimeSpan(0, 0, 0, 10);

    public static TimeSpan EpisodeDataCacheTime = new TimeSpan(0, 1, 0, 0);
    //public static TimeSpan EpisodeListCacheTime = new TimeSpan(0, 1, 0, 0);
    public static TimeSpan EpisodeListCacheTime = new TimeSpan(0, 4, 0, 0);
    public static TimeSpan EpisodeSongListCacheTime = new TimeSpan(7, 0, 0, 0);
    public static TimeSpan EpisodeListFullSynchronzingIntervall = new TimeSpan(7, 0, 0, 0);

    public static TimeSpan ScheduledEpisodeDataCacheTime = new TimeSpan(0, 1, 0, 0);

    public static TimeSpan ProgramDataCacheTime = new TimeSpan(1, 0, 0, 0);
    public static TimeSpan ProgramListCacheTime = new TimeSpan(1, 0, 0, 0);
    public static TimeSpan ChannelListCacheTime = new TimeSpan(1, 0, 0, 0);

    public static bool IsTimeOut(DateTimeOffset time, TimeSpan timeout)
    {
        var diff = time - DateTimeOffset.UtcNow;

        if (Math.Abs(diff.Ticks) > timeout.Ticks)
        {
            return true;
        }

        return false;
    }
}
