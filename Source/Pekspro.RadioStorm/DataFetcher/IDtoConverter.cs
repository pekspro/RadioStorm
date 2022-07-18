namespace Pekspro.RadioStorm.DataFetcher
{
    public interface IDtoConverter
    {
        ChannelData Convert(Channel c);
        ChannelStatusData Convert(ChannelRightNow scheduleData);
        EpisodeData Convert(Episode e);
        IEnumerable<ChannelStatusData> Convert(ICollection<ChannelRightNow> channels);
        ChannelData[] Convert(IEnumerable<Channel> channels);
        EpisodeData[] Convert(IEnumerable<Episode> episodes);
        ProgramData[] Convert(IEnumerable<Program> programs);
        List<ScheduledEpisodeListItemData> Convert(int channelId, DateTimeOffset date, ICollection<ScheduledEpisode> schedule);
        ScheduledEpisodeListItemData Convert(int channelId, DateTimeOffset date, ScheduledEpisode schedule);
        ProgramData Convert(Program p);
        ChannelSongListItemData ConvertToChannelSong(int channelId, Song s);
        List<ChannelSongListItemData> ConvertToChannelSongs(int channelId, ICollection<Song> songs);
        EpisodeSongListItemData ConvertToEpisodeSong(int episodeId, Song s);
        List<EpisodeSongListItemData> ConvertToEpisodeSongs(int episodeId, ICollection<Song> songs);
    }
}