namespace Pekspro.RadioStorm.Utilities;

/// <summary>
/// Typed bulk operations that select the matching <see cref="SqliteBulkMap{T}"/> for each entity,
/// so callers don't have to pass a map explicitly.
/// </summary>
public static class BulkMapExtensions
{
    #region Cache database - insert or update

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<EpisodeData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.EpisodeDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<EpisodeListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.EpisodeListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<ListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.ListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<ProgramData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.ProgramDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<ChannelData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.ChannelDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<EpisodeSongListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.EpisodeSongListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<ChannelSongListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.ChannelSongListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<ScheduledEpisodeListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.ScheduledEpisodeListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertOrUpdateAsync(this CacheDatabaseContext context, IEnumerable<ChannelStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, CacheDatabaseBulkMaps.ChannelStatusDataMap, cancellationToken);

    #endregion

    #region Cache database - insert

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<EpisodeData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.EpisodeDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<EpisodeListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.EpisodeListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ProgramData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ProgramDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ChannelData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ChannelDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<EpisodeSongListItemData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.EpisodeSongListItemDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<EpisodeSongListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.EpisodeSongListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ChannelSongListItemData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ChannelSongListItemDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ChannelSongListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ChannelSongListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ScheduledEpisodeListItemData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ScheduledEpisodeListItemDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ScheduledEpisodeListSyncStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ScheduledEpisodeListSyncStatusDataMap, cancellationToken);

    public static Task BulkInsertAsync(this CacheDatabaseContext context, IEnumerable<ChannelStatusData> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, CacheDatabaseBulkMaps.ChannelStatusDataMap, cancellationToken);

    #endregion

    #region General database

    public static Task BulkInsertOrUpdateAsync(this GeneralDatabaseContext context, IEnumerable<DownloadState> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertOrUpdateAsync(entities, GeneralDatabaseBulkMaps.DownloadStateMap, cancellationToken);

    public static Task BulkInsertAsync(this GeneralDatabaseContext context, IEnumerable<DownloadState> entities, CancellationToken cancellationToken = default)
        => context.BulkInsertAsync(entities, GeneralDatabaseBulkMaps.DownloadStateMap, cancellationToken);

    #endregion
}
