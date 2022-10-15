namespace Pekspro.RadioStorm.DataFetcher;

internal sealed class DtoConverter : IDtoConverter
{
    public ChannelData Convert(Channel c) => new ChannelData
    {
        ChannelColor = c.Color,
        ChannelId = c.Id,
        ChannelImageHighResolution = c.Imagetemplate,
        ChannelImageLowResolution = c.Image,
        ChannelGroupName = c.Channeltype,
        Title = c.Name ?? "?",
        LiveAudioUrl = c.Liveaudio?.Url,
        WebPageUri = c.Siteurl
    };

    public ChannelData[] Convert(IEnumerable<Channel> channels) =>
        channels.Select(a => Convert(a)).ToArray();

    public EpisodeData Convert(Episode e) => new EpisodeData
    {
        EpisodeId = e.Id,
        Description = e.Description ?? "?",
        EpisodeImage = e.Imageurl,
        PublishDate = e.Publishdateutc,
        ProgramName = e.Program?.Name,
        ProgramId = e.Program?.Id,
        AudioDownloadUrl = e.Downloadpodfile?.Url,
        AudioDownloadDuration = e.Downloadpodfile?.Duration ?? 0,
        AudioStreamWithoutMusicUrl = e.Listenpodfile?.Url,
        AudioStreamWithoutMusicDuration = e.Listenpodfile?.Duration ?? 0,
        Title = e.Title ?? "?",
        AudioStreamWithMusicUrl = e.Broadcast?.Broadcastfiles?.Any() == true ? e.Broadcast.Broadcastfiles.First().Url : null,
        AudioStreamWithMusicDuration = e.Broadcast?.Broadcastfiles?.Any() == true ? e.Broadcast.Broadcastfiles.First().Duration : 0,
        AudioStreamWithMusicExpireDate = e.Broadcast?.Availablestoputc
    };

    public EpisodeData[] Convert(IEnumerable<Episode> episodes) =>
        episodes.Select(a => Convert(a)).ToArray();

    public ProgramData Convert(Program p) => new ProgramData
    {
        CategoryId = p.Programcategory?.Id ?? 0,
        CategoryName = p.Programcategory?.Name,
        Description = p.Description ?? "?",
        ProgramId = p.Id,
        ProgramImageHighResolution = p.Programimagetemplate,
        ProgramImageLowResolution = p.Programimage,
        Name = p.Name ?? "?",
        Archived = p.Archived,
        HasOnDemand = p.Hasondemand,
        HasPod = p.Haspod,
        BroadcastInfo = p.Broadcastinfo,
        ProgramUri = p.Programurl,
        ChannelId = p.Channel?.Id,
        FacebookPageUri = p.Socialmediaplatforms?.FirstOrDefault(a => a.Platform == "Facebook")?.Platformurl,
        TwitterPageUri = p.Socialmediaplatforms?.FirstOrDefault(a => a.Platform == "Twitter")?.Platformurl
    };

    public ProgramData[] Convert(IEnumerable<Program> programs) =>
        programs.Select(a => Convert(a)).ToArray();

    public ChannelStatusData Convert(ChannelRightNow scheduleData) => new ChannelStatusData
    {
        ChannelId = scheduleData.Id,

        CurrentProgramId = scheduleData.Currentscheduledepisode?.Program?.Id,
        CurrentProgram = scheduleData.Currentscheduledepisode?.Program?.Name ?? scheduleData.Currentscheduledepisode?.Title,
        CurrentProgramImage = scheduleData.Currentscheduledepisode?.Socialimage,
        CurrentProgramDescription = scheduleData.Currentscheduledepisode?.Description,
        CurrentStartTime = scheduleData.Currentscheduledepisode?.Starttimeutc,
        CurrentEndTime = scheduleData.Currentscheduledepisode?.Endtimeutc,

        NextProgramId = scheduleData.Nextscheduledepisode?.Program?.Id,
        NextProgram = scheduleData.Nextscheduledepisode?.Program?.Name ?? scheduleData.Nextscheduledepisode?.Title,
        NextProgramImage = scheduleData.Nextscheduledepisode?.Socialimage,
        NextProgramDescription = scheduleData.Nextscheduledepisode?.Description,
        NextStartTime = scheduleData.Nextscheduledepisode?.Starttimeutc,
        NextEndTime = scheduleData.Nextscheduledepisode?.Endtimeutc
    };

    public IEnumerable<ChannelStatusData> Convert(ICollection<ChannelRightNow> channels) =>
        channels.Select(a => Convert(a)).ToArray();

    public List<EpisodeSongListItemData> ConvertToEpisodeSongs(int episodeId, ICollection<Song> songs) =>
        songs.Select(a => ConvertToEpisodeSong(episodeId, a)).ToList();

    public EpisodeSongListItemData ConvertToEpisodeSong(int episodeId, Song s) => new EpisodeSongListItemData
    {
        EpisodeId = episodeId,
        Title = s.Title ?? "?",
        Artist = s.Artist,
        AlbumName = s.Albumname,
        Composer = s.Composer,
        PublishDate = s.Starttimeutc
    };

    public List<ChannelSongListItemData> ConvertToChannelSongs(int channelId, ICollection<Song> songs) =>
        songs.Select(a => ConvertToChannelSong(channelId, a)).ToList();

    public ChannelSongListItemData ConvertToChannelSong(int channelId, Song s) => new ChannelSongListItemData
    {
        ChannelId = channelId,
        Title = s.Title ?? "?",
        Artist = s.Artist,
        AlbumName = s.Albumname,
        Composer = s.Composer,
        PublishDate = s.Starttimeutc
    };

    public List<ScheduledEpisodeListItemData> Convert(int channelId, DateTimeOffset date, ICollection<ScheduledEpisode> schedule) =>
        schedule.Select(a => Convert(channelId, date, a)).ToList();

    public ScheduledEpisodeListItemData Convert(int channelId, DateTimeOffset date, ScheduledEpisode schedule) => new ScheduledEpisodeListItemData()
    {
        ChannelId = channelId,
        Date = date,

        EpisodeId = schedule.Episodeid ?? -1,
        Title = schedule.Title ?? "?",
        Description = schedule.Description ?? "?",
        StartTimeUtc = schedule.Starttimeutc,
        EndTimeUtc = schedule.Endtimeutc,
        ProgramId = schedule.Program?.Id ?? 0,
        ProgramName = schedule.Program?.Name
    };
}
