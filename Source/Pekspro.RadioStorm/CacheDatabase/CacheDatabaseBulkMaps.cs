namespace Pekspro.RadioStorm.CacheDatabase;

/// <summary>
/// Reflection-free bulk maps for the cache database tables.
/// Must be kept in sync with <see cref="CacheDatabaseContext"/> and its migrations.
/// Date values are stored as UTC ticks, matching <see cref="DateTimeOffsetToTicksConverter"/>.
/// </summary>
internal static class CacheDatabaseBulkMaps
{
    public static SqliteBulkMap<EpisodeData> EpisodeDataMap { get; } = new EpisodeDataBulkMap();
    public static SqliteBulkMap<EpisodeListSyncStatusData> EpisodeListSyncStatusDataMap { get; } = new EpisodeListSyncStatusDataBulkMap();
    public static SqliteBulkMap<ListSyncStatusData> ListSyncStatusDataMap { get; } = new ListSyncStatusDataBulkMap();
    public static SqliteBulkMap<ProgramData> ProgramDataMap { get; } = new ProgramDataBulkMap();
    public static SqliteBulkMap<ChannelData> ChannelDataMap { get; } = new ChannelDataBulkMap();
    public static SqliteBulkMap<EpisodeSongListItemData> EpisodeSongListItemDataMap { get; } = new EpisodeSongListItemDataBulkMap();
    public static SqliteBulkMap<EpisodeSongListSyncStatusData> EpisodeSongListSyncStatusDataMap { get; } = new EpisodeSongListSyncStatusDataBulkMap();
    public static SqliteBulkMap<ChannelSongListItemData> ChannelSongListItemDataMap { get; } = new ChannelSongListItemDataBulkMap();
    public static SqliteBulkMap<ChannelSongListSyncStatusData> ChannelSongListSyncStatusDataMap { get; } = new ChannelSongListSyncStatusDataBulkMap();
    public static SqliteBulkMap<ScheduledEpisodeListItemData> ScheduledEpisodeListItemDataMap { get; } = new ScheduledEpisodeListItemDataBulkMap();
    public static SqliteBulkMap<ScheduledEpisodeListSyncStatusData> ScheduledEpisodeListSyncStatusDataMap { get; } = new ScheduledEpisodeListSyncStatusDataBulkMap();
    public static SqliteBulkMap<ChannelStatusData> ChannelStatusDataMap { get; } = new ChannelStatusDataBulkMap();

    private static object Ticks(DateTimeOffset value) => value.UtcTicks;

    private static object? Ticks(DateTimeOffset? value) => value?.UtcTicks;

    private sealed class EpisodeDataBulkMap() : SqliteBulkMap<EpisodeData>(
        nameof(EpisodeData),
        [nameof(EpisodeData.EpisodeId)],
        [
            nameof(EpisodeData.EpisodeId),
            nameof(EpisodeData.ProgramId),
            nameof(EpisodeData.ProgramName),
            nameof(EpisodeData.AudioStreamWithMusicUrl),
            nameof(EpisodeData.AudioStreamWithoutMusicUrl),
            nameof(EpisodeData.AudioDownloadUrl),
            nameof(EpisodeData.AudioStreamWithMusicDuration),
            nameof(EpisodeData.AudioStreamWithoutMusicDuration),
            nameof(EpisodeData.AudioDownloadDuration),
            nameof(EpisodeData.PublishDate),
            nameof(EpisodeData.AudioStreamWithMusicExpireDate),
            nameof(EpisodeData.Title),
            nameof(EpisodeData.Description),
            nameof(EpisodeData.EpisodeImage),
            nameof(EpisodeData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(EpisodeData e, Span<object?> v)
        {
            v[0] = e.EpisodeId;
            v[1] = e.ProgramId;
            v[2] = e.ProgramName;
            v[3] = e.AudioStreamWithMusicUrl;
            v[4] = e.AudioStreamWithoutMusicUrl;
            v[5] = e.AudioDownloadUrl;
            v[6] = e.AudioStreamWithMusicDuration;
            v[7] = e.AudioStreamWithoutMusicDuration;
            v[8] = e.AudioDownloadDuration;
            v[9] = Ticks(e.PublishDate);
            v[10] = Ticks(e.AudioStreamWithMusicExpireDate);
            v[11] = e.Title;
            v[12] = e.Description;
            v[13] = e.EpisodeImage;
            v[14] = Ticks(e.LatestUpdateTime);
        }
    }

    private sealed class EpisodeListSyncStatusDataBulkMap() : SqliteBulkMap<EpisodeListSyncStatusData>(
        nameof(EpisodeListSyncStatusData),
        [nameof(EpisodeListSyncStatusData.ProgramId)],
        [
            nameof(EpisodeListSyncStatusData.ProgramId),
            nameof(EpisodeListSyncStatusData.Status),
            nameof(EpisodeListSyncStatusData.IncrementallyUpdateCount),
            nameof(EpisodeListSyncStatusData.LatestUpdateTime),
            nameof(EpisodeListSyncStatusData.LatestFullSynchronizingTime),
        ])
    {
        public override void WriteValues(EpisodeListSyncStatusData e, Span<object?> v)
        {
            v[0] = e.ProgramId;
            v[1] = (int)e.Status;
            v[2] = e.IncrementallyUpdateCount;
            v[3] = Ticks(e.LatestUpdateTime);
            v[4] = Ticks(e.LatestFullSynchronizingTime);
        }
    }

    private sealed class ListSyncStatusDataBulkMap() : SqliteBulkMap<ListSyncStatusData>(
        nameof(ListSyncStatusData),
        [nameof(ListSyncStatusData.TypeId)],
        [
            nameof(ListSyncStatusData.TypeId),
            nameof(ListSyncStatusData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ListSyncStatusData e, Span<object?> v)
        {
            v[0] = (int)e.TypeId;
            v[1] = Ticks(e.LatestUpdateTime);
        }
    }

    private sealed class ProgramDataBulkMap() : SqliteBulkMap<ProgramData>(
        nameof(ProgramData),
        [nameof(ProgramData.ProgramId)],
        [
            nameof(ProgramData.ProgramId),
            nameof(ProgramData.Name),
            nameof(ProgramData.CategoryId),
            nameof(ProgramData.CategoryName),
            nameof(ProgramData.Description),
            nameof(ProgramData.ProgramImageHighResolution),
            nameof(ProgramData.ProgramImageLowResolution),
            nameof(ProgramData.Archived),
            nameof(ProgramData.HasOnDemand),
            nameof(ProgramData.HasPod),
            nameof(ProgramData.BroadcastInfo),
            nameof(ProgramData.ProgramUri),
            nameof(ProgramData.FacebookPageUri),
            nameof(ProgramData.TwitterPageUri),
            nameof(ProgramData.ChannelId),
            nameof(ProgramData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ProgramData e, Span<object?> v)
        {
            v[0] = e.ProgramId;
            v[1] = e.Name;
            v[2] = e.CategoryId;
            v[3] = e.CategoryName;
            v[4] = e.Description;
            v[5] = e.ProgramImageHighResolution;
            v[6] = e.ProgramImageLowResolution;
            v[7] = e.Archived;
            v[8] = e.HasOnDemand;
            v[9] = e.HasPod;
            v[10] = e.BroadcastInfo;
            v[11] = e.ProgramUri;
            v[12] = e.FacebookPageUri;
            v[13] = e.TwitterPageUri;
            v[14] = e.ChannelId;
            v[15] = Ticks(e.LatestUpdateTime);
        }
    }

    private sealed class ChannelDataBulkMap() : SqliteBulkMap<ChannelData>(
        nameof(ChannelData),
        [nameof(ChannelData.ChannelId)],
        [
            nameof(ChannelData.ChannelId),
            nameof(ChannelData.Title),
            nameof(ChannelData.ChannelGroupName),
            nameof(ChannelData.ChannelImageLowResolution),
            nameof(ChannelData.ChannelImageHighResolution),
            nameof(ChannelData.ChannelColor),
            nameof(ChannelData.LiveAudioUrl),
            nameof(ChannelData.WebPageUri),
            nameof(ChannelData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ChannelData e, Span<object?> v)
        {
            v[0] = e.ChannelId;
            v[1] = e.Title;
            v[2] = e.ChannelGroupName;
            v[3] = e.ChannelImageLowResolution;
            v[4] = e.ChannelImageHighResolution;
            v[5] = e.ChannelColor;
            v[6] = e.LiveAudioUrl;
            v[7] = e.WebPageUri;
            v[8] = Ticks(e.LatestUpdateTime);
        }
    }

    // EpisodeSongListItemDataId is generated by the database and therefore not written.
    private sealed class EpisodeSongListItemDataBulkMap() : SqliteBulkMap<EpisodeSongListItemData>(
        nameof(EpisodeSongListItemData),
        [nameof(EpisodeSongListItemData.EpisodeSongListItemDataId)],
        [
            nameof(EpisodeSongListItemData.EpisodeId),
            nameof(EpisodeSongListItemData.Title),
            nameof(EpisodeSongListItemData.Artist),
            nameof(EpisodeSongListItemData.AlbumName),
            nameof(EpisodeSongListItemData.Composer),
            nameof(EpisodeSongListItemData.PublishDate),
            nameof(EpisodeSongListItemData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(EpisodeSongListItemData e, Span<object?> v)
        {
            v[0] = e.EpisodeId;
            v[1] = e.Title;
            v[2] = e.Artist;
            v[3] = e.AlbumName;
            v[4] = e.Composer;
            v[5] = Ticks(e.PublishDate);
            v[6] = Ticks(e.LatestUpdateTime);
        }
    }

    private sealed class EpisodeSongListSyncStatusDataBulkMap() : SqliteBulkMap<EpisodeSongListSyncStatusData>(
        nameof(EpisodeSongListSyncStatusData),
        [nameof(EpisodeSongListSyncStatusData.EpisodeId)],
        [
            nameof(EpisodeSongListSyncStatusData.EpisodeId),
            nameof(EpisodeSongListSyncStatusData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(EpisodeSongListSyncStatusData e, Span<object?> v)
        {
            v[0] = e.EpisodeId;
            v[1] = Ticks(e.LatestUpdateTime);
        }
    }

    // ChannelSongListItemDataId is generated by the database and therefore not written.
    private sealed class ChannelSongListItemDataBulkMap() : SqliteBulkMap<ChannelSongListItemData>(
        nameof(ChannelSongListItemData),
        [nameof(ChannelSongListItemData.ChannelSongListItemDataId)],
        [
            nameof(ChannelSongListItemData.ChannelId),
            nameof(ChannelSongListItemData.Title),
            nameof(ChannelSongListItemData.Artist),
            nameof(ChannelSongListItemData.AlbumName),
            nameof(ChannelSongListItemData.Composer),
            nameof(ChannelSongListItemData.PublishDate),
            nameof(ChannelSongListItemData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ChannelSongListItemData e, Span<object?> v)
        {
            v[0] = e.ChannelId;
            v[1] = e.Title;
            v[2] = e.Artist;
            v[3] = e.AlbumName;
            v[4] = e.Composer;
            v[5] = Ticks(e.PublishDate);
            v[6] = Ticks(e.LatestUpdateTime);
        }
    }

    private sealed class ChannelSongListSyncStatusDataBulkMap() : SqliteBulkMap<ChannelSongListSyncStatusData>(
        nameof(ChannelSongListSyncStatusData),
        [nameof(ChannelSongListSyncStatusData.ChannelId)],
        [
            nameof(ChannelSongListSyncStatusData.ChannelId),
            nameof(ChannelSongListSyncStatusData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ChannelSongListSyncStatusData e, Span<object?> v)
        {
            v[0] = e.ChannelId;
            v[1] = Ticks(e.LatestUpdateTime);
        }
    }

    // ScheduledEpisodeDataId is generated by the database and therefore not written.
    private sealed class ScheduledEpisodeListItemDataBulkMap() : SqliteBulkMap<ScheduledEpisodeListItemData>(
        nameof(ScheduledEpisodeListItemData),
        [nameof(ScheduledEpisodeListItemData.ScheduledEpisodeDataId)],
        [
            nameof(ScheduledEpisodeListItemData.ChannelId),
            nameof(ScheduledEpisodeListItemData.Date),
            nameof(ScheduledEpisodeListItemData.EpisodeId),
            nameof(ScheduledEpisodeListItemData.Title),
            nameof(ScheduledEpisodeListItemData.Description),
            nameof(ScheduledEpisodeListItemData.StartTimeUtc),
            nameof(ScheduledEpisodeListItemData.EndTimeUtc),
            nameof(ScheduledEpisodeListItemData.ProgramId),
            nameof(ScheduledEpisodeListItemData.ProgramName),
        ])
    {
        public override void WriteValues(ScheduledEpisodeListItemData e, Span<object?> v)
        {
            v[0] = e.ChannelId;
            v[1] = Ticks(e.Date);
            v[2] = e.EpisodeId;
            v[3] = e.Title;
            v[4] = e.Description;
            v[5] = Ticks(e.StartTimeUtc);
            v[6] = Ticks(e.EndTimeUtc);
            v[7] = e.ProgramId;
            v[8] = e.ProgramName;
        }
    }

    private sealed class ScheduledEpisodeListSyncStatusDataBulkMap() : SqliteBulkMap<ScheduledEpisodeListSyncStatusData>(
        nameof(ScheduledEpisodeListSyncStatusData),
        [nameof(ScheduledEpisodeListSyncStatusData.ChannelId)],
        [
            nameof(ScheduledEpisodeListSyncStatusData.ChannelId),
            nameof(ScheduledEpisodeListSyncStatusData.Date),
            nameof(ScheduledEpisodeListSyncStatusData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ScheduledEpisodeListSyncStatusData e, Span<object?> v)
        {
            v[0] = e.ChannelId;
            v[1] = Ticks(e.Date);
            v[2] = Ticks(e.LatestUpdateTime);
        }
    }

    private sealed class ChannelStatusDataBulkMap() : SqliteBulkMap<ChannelStatusData>(
        nameof(ChannelStatusData),
        [nameof(ChannelStatusData.ChannelId)],
        [
            nameof(ChannelStatusData.ChannelId),
            nameof(ChannelStatusData.CurrentProgramId),
            nameof(ChannelStatusData.CurrentProgram),
            nameof(ChannelStatusData.CurrentProgramImage),
            nameof(ChannelStatusData.CurrentProgramDescription),
            nameof(ChannelStatusData.CurrentStartTime),
            nameof(ChannelStatusData.CurrentEndTime),
            nameof(ChannelStatusData.NextProgramId),
            nameof(ChannelStatusData.NextProgram),
            nameof(ChannelStatusData.NextProgramImage),
            nameof(ChannelStatusData.NextProgramDescription),
            nameof(ChannelStatusData.NextStartTime),
            nameof(ChannelStatusData.NextEndTime),
            nameof(ChannelStatusData.LatestUpdateTime),
        ])
    {
        public override void WriteValues(ChannelStatusData e, Span<object?> v)
        {
            v[0] = e.ChannelId;
            v[1] = e.CurrentProgramId;
            v[2] = e.CurrentProgram;
            v[3] = e.CurrentProgramImage;
            v[4] = e.CurrentProgramDescription;
            v[5] = Ticks(e.CurrentStartTime);
            v[6] = Ticks(e.CurrentEndTime);
            v[7] = e.NextProgramId;
            v[8] = e.NextProgram;
            v[9] = e.NextProgramImage;
            v[10] = e.NextProgramDescription;
            v[11] = Ticks(e.NextStartTime);
            v[12] = Ticks(e.NextEndTime);
            v[13] = Ticks(e.LatestUpdateTime);
        }
    }
}
