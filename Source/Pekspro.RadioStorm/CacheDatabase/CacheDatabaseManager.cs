using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pekspro.RadioStorm.CacheDatabase;

public sealed class CacheDatabaseManager
{
    #region Private properties

    private ICacheDatabaseContextFactory CacheDatabaseContextFactory { get; }
    private IDateTimeProvider DateTimeProvider { get; }
    private ILogger Logger { get; }

    #endregion

    #region Constructor

    public CacheDatabaseManager(
        ICacheDatabaseContextFactory cacheDatabaseContextFactory,
        IDateTimeProvider dateTimeProvider,
        ILogger<CacheDatabaseManager> logger)
    {
        CacheDatabaseContextFactory = cacheDatabaseContextFactory;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;
    }

    #endregion

    #region Methods

    private void Log(string text)
    {
        Logger.LogTrace(text);
    }

    private void LogError(string methodName, Exception ex)
    {
        string latestDatabaseError = $"Exception from database in {methodName}: {ex.GetType()} {ex.Message}";

        Logger.LogError(ex, latestDatabaseError);
    }

    public async Task ClearAsync()
    {
        Log($"{nameof(ClearAsync)} Start");

        try
        {
            await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using var trans = databaseContext.Database.BeginTransaction();

                databaseContext.EpisodeData.Where(a => true).BatchDelete();
                databaseContext.ProgramData.Where(a => true).BatchDelete();
                databaseContext.ChannelData.Where(a => true).BatchDelete();
                databaseContext.EpisodeListSyncStatusData.Where(a => true).BatchDelete();
                databaseContext.ListSyncStatusData.Where(a => true).BatchDelete();
                databaseContext.ScheduledEpisodeListSyncStatusData.Where(a => true).BatchDelete();

                databaseContext.EpisodeSongListItemData.Where(a => true).BatchDelete();
                databaseContext.EpisodeSongListSyncStatusData.Where(a => true).BatchDelete();
                databaseContext.ChannelSongListItemData.Where(a => true).BatchDelete();
                databaseContext.ChannelSongListSyncStatusData.Where(a => true).BatchDelete();
                databaseContext.ChannelStatusData.Where(a => true).BatchDelete();
                databaseContext.ScheduledEpisodeListItemData.Where(a => true).BatchDelete();

                trans.Commit();
            }).ConfigureAwait(false);
        }
        catch (Exception)
        {

        }

        Log($"{nameof(ClearAsync)} End");
    }

    internal async Task InsertOrUpdateEpisodeAsync(EpisodeData data, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateEpisodeAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                data.LatestUpdateTime = DateTimeProvider.OffsetNow;

                await databaseContext.BulkInsertOrUpdateAsync(new List<EpisodeData>() { data }, cancellationToken: cancellationToken).ConfigureAwait(false);
                
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateEpisodeAsync), ex);

            throw;
        }

        Log($"{nameof(InsertOrUpdateEpisodeAsync)} End");
    }

    internal async Task InsertOrUpdateEpisodesAsync(int programId, IList<EpisodeData> episodes, EpisodeListSyncStatusData p, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateEpisodesAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using (var transaction = databaseContext.Database.BeginTransaction())
                {
                    var now = DateTimeProvider.OffsetNow;
                    p.LatestUpdateTime = now;
                    if (p.Status == EpisodeListSyncStatusData.SyncStatus.FullySynchronized)
                    {
                        p.LatestFullSynchronizingTime = now;
                    }

                    //Delete existing?
                    if (p.Status == EpisodeListSyncStatusData.SyncStatus.FullySynchronized)
                    {
                        await databaseContext.EpisodeData.Where(a => a.ProgramId == programId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);
                    }

                    foreach (var data in episodes)
                    {
                        data.LatestUpdateTime = now;
                        //connection.InsertOrReplace(data);
                    }

                    await databaseContext.BulkInsertOrUpdateAsync(episodes, cancellationToken: cancellationToken);
                    await databaseContext.BulkInsertOrUpdateAsync(new List<EpisodeListSyncStatusData>() { p }, cancellationToken: cancellationToken).ConfigureAwait(false);

                    transaction.Commit();
                };
            }).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateEpisodesAsync), ex);

            Log($"Failed to insert/update episodes for {programId}, error: {ex.Message}");

            throw;
        }

        Log($"{nameof(InsertOrUpdateEpisodesAsync)} End");
    }

    internal async Task InsertOrUpdateEpisodesAsync(IList<EpisodeData> episodes, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateEpisodesAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                var now = DateTimeProvider.OffsetNow;

                foreach (var data in episodes)
                {
                    data.LatestUpdateTime = now;
                }

                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                await databaseContext.BulkInsertOrUpdateAsync(episodes, cancellationToken: cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateEpisodesAsync), ex);

            throw;
        }

        Log($"{nameof(InsertOrUpdateEpisodesAsync)} End");
    }

    public async Task<EpisodeData?> GetEpisodeAsync(int episodeId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeData
                        .Where(e => e.EpisodeId == episodeId)
                        .FirstOrDefault();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetEpisodeAsync), ex);

            throw;
        }
    }

    public async Task<EpisodeData?> GetNextEpisodeAsync(int programId, DateTimeOffset currentPublishDate, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeData
                        .Where(e => e.ProgramId == programId && e.PublishDate > currentPublishDate)
                        .OrderBy(e => e.PublishDate)
                        .FirstOrDefault();

            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetNextEpisodeAsync), ex);

            throw;
        }
    }

    public async Task<EpisodeData?> GetPreviousEpisodeAsync(int programId, DateTimeOffset currentPublishDate, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeData
                    .Where(e => e.ProgramId == programId && e.PublishDate < currentPublishDate)
                    .OrderByDescending(e => e.PublishDate)
                    .FirstOrDefault();

            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetPreviousEpisodeAsync), ex);

            throw;
        }
    }

    public async Task<EpisodeData?> GetFirstMatchingEpisodeAsync
    (
        int?[] validProgramId,
        int[] episodesToIgnore,
        bool orderAscending,
        CancellationToken cancellation
    )
    {
        try
        {
            if (validProgramId is null || validProgramId.Length <= 0)
            {
                return null;
            }

            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var query = databaseContext.EpisodeData.Where(a => validProgramId.Contains(a.ProgramId));
                query = query.Where(a => a.AudioDownloadUrl != null || a.AudioStreamWithMusicUrl != null || a.AudioStreamWithoutMusicUrl != null);

                if (episodesToIgnore is not null)
                {
                    query = query.Where(a => !episodesToIgnore.Contains(a.EpisodeId));
                }

                if (orderAscending)
                {
                    query = query.OrderBy(a => a.PublishDate);
                }
                else
                {
                    query = query.OrderByDescending(a => a.PublishDate);
                }

                return query.FirstOrDefault();
            }, cancellation).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetFirstMatchingEpisodeAsync), ex);

            throw;
        }
    }

    public async Task<EpisodeData?> GetLatestEpisodeAsync(int programId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeData
                    .Where(e => e.ProgramId == programId)
                    .OrderByDescending(e => e.PublishDate)
                    .FirstOrDefault();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetLatestEpisodeAsync), ex);

            throw;
        }
    }

    public async Task<IList<EpisodeData>?> GetEpisodesAsync(int programId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeData
                    .Where(e => e.ProgramId == programId)
                    .ToList();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetEpisodesAsync), ex);

            throw;
        }
    }

    public async Task<IList<EpisodeData>?> GetEpisodesAsync(int[] episodeIds, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeData
                    .Where(e => episodeIds.Contains(e.EpisodeId))
                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetEpisodesAsync), ex);

            throw;
        }
    }

    internal async Task InsertOrUpdateProgramAsync(ProgramData data, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateProgramAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                data.LatestUpdateTime = DateTimeProvider.OffsetNow;
                await databaseContext.BulkInsertOrUpdateAsync(new List<ProgramData>() { data }, cancellationToken: cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateProgramAsync), ex);
        }

        Log($"{nameof(InsertOrUpdateProgramAsync)} End");
    }

    internal async Task InsertOrUpdateProgramsAsync(IList<ProgramData> programs, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateProgramsAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using var tran = databaseContext.Database.BeginTransaction();

                var now = DateTimeProvider.OffsetNow;
                foreach (var data in programs)
                {
                    data.LatestUpdateTime = now;
                }

                ListSyncStatusData syncStats = new ListSyncStatusData()
                {
                    LatestUpdateTime = now,
                    TypeId = ListSyncStatusData.ListType.Programs
                };

                await databaseContext.ProgramData.Where(a => true).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);
                await databaseContext.BulkInsertOrUpdateAsync(programs, cancellationToken: cancellationToken).ConfigureAwait(false);
                await databaseContext.BulkInsertOrUpdateAsync(new List<ListSyncStatusData>() { syncStats }, cancellationToken: cancellationToken).ConfigureAwait(false);

                tran.Commit();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateProgramsAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(InsertOrUpdateProgramsAsync)} End");
        }
    }


    public async Task<ProgramData?> GetProgramAsync(int programId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ProgramData
                    .Where(e => e.ProgramId == programId)
                    .FirstOrDefault();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetProgramAsync), ex);

            throw;
        }
    }

    public async Task<IList<ProgramData>?> GetProgramsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ProgramData.ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetProgramsAsync), ex);

            throw;
        }
    }

    public async Task<DateTimeOffset> GetLatestListSyncTimeAsync(ListSyncStatusData.ListType typeId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var syncData = databaseContext.ListSyncStatusData
                    .Where(e => e.TypeId == typeId)
                    .FirstOrDefault();

                if (syncData is null)
                {
                    return DateTimeOffset.MinValue;
                }

                return syncData.LatestUpdateTime;

            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetLatestListSyncTimeAsync), ex);

            throw;
        }
    }

    public async Task<EpisodeListSyncStatusData?> GetEpisodeListSyncStatusAsync(int programId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeListSyncStatusData
                    .Where(e => e.ProgramId == programId)
                    .FirstOrDefault();            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetEpisodeListSyncStatusAsync), ex);

            throw;
        }
    }

    internal async Task InsertOrUpdateChannelAsync(ChannelData data, CancellationToken cancellationToken)
    {
        Log($"{InsertOrUpdateChannelAsync} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                data.LatestUpdateTime = DateTimeProvider.OffsetNow;
                await databaseContext.BulkInsertOrUpdateAsync(new List<ChannelData>() { data }, cancellationToken: cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateChannelAsync), ex);

            throw;
        }

        Log($"{InsertOrUpdateChannelAsync} End");
    }

    internal async Task InsertOrUpdateChannelsAsync(IList<ChannelData> channels, CancellationToken cancellationToken)
    {
        Log($"{InsertOrUpdateChannelsAsync} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using var tran = databaseContext.Database.BeginTransaction();

                var now = DateTimeProvider.OffsetNow;
                foreach (var data in channels)
                {
                    data.LatestUpdateTime = now;
                }

                ListSyncStatusData syncStats = new ListSyncStatusData()
                {
                    LatestUpdateTime = now,
                    TypeId = ListSyncStatusData.ListType.Channels
                };

                await databaseContext.ChannelData.Where(a => true).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);
                await databaseContext.BulkInsertOrUpdateAsync(channels, cancellationToken: cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertOrUpdateAsync(new List<ListSyncStatusData>() { syncStats }, cancellationToken: cancellationToken).ConfigureAwait(false);

                tran.Commit();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateChannelsAsync), ex);

            throw;
        }
        finally
        {
            Log($"{InsertOrUpdateChannelsAsync} End");
        }
    }


    public async Task<ChannelData?> GetChannelAsync(int channelId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ChannelData
                        .Where(e => e.ChannelId == channelId)
                        .FirstOrDefault();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetChannelAsync), ex);

            throw;
        }
    }

    public async Task<IList<ChannelData>?> GetChannelsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ChannelData.ToList();
            }, cancellationToken).ConfigureAwait(false);            
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetChannelsAsync), ex);

            throw;
        }
    }

    internal async Task InsertOrUpdateEpisodesSongListItemsAsync(int episodeId, IList<EpisodeSongListItemData> songs, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateEpisodesSongListItemsAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var now = DateTimeProvider.OffsetNow;
                foreach (var data in songs)
                {
                    data.EpisodeId = episodeId;
                    data.LatestUpdateTime = now;
                }

                using var tran = databaseContext.Database.BeginTransaction();

                //Delete existing
                await databaseContext.EpisodeSongListItemData.Where(a => a.EpisodeId == episodeId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertAsync(songs, cancellationToken: cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertOrUpdateAsync(new List<EpisodeSongListSyncStatusData>() {
                    new EpisodeSongListSyncStatusData()
                    {
                        EpisodeId = episodeId,
                        LatestUpdateTime = now
                    }
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

                tran.Commit();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateEpisodesSongListItemsAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(InsertOrUpdateEpisodesSongListItemsAsync)} End");
        }
    }

    public async Task<IList<EpisodeSongListItemData>?> GetEpisodesSongListItemsAsync(int episodeId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.EpisodeSongListItemData
                    .Where(e => e.EpisodeId == episodeId)
                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetEpisodesSongListItemsAsync), ex);

            throw;
        }
    }

    public async Task<DateTimeOffset> GetLatestEpisodeSongListSyncTimeAsync(int episodeId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var songListSyncData = databaseContext.EpisodeSongListSyncStatusData
                        .Where(e => e.EpisodeId == episodeId)
                        .FirstOrDefault();

                if (songListSyncData is not null)
                {
                    return songListSyncData.LatestUpdateTime;
                }

                return DateTimeOffset.MinValue;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetLatestEpisodeSongListSyncTimeAsync), ex);

            throw;
        }
    }

    public async Task DeleteEpisodeSongListAsync(int episodeId, CancellationToken cancellationToken)
    {
        Log($"{nameof(DeleteEpisodeSongListAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using var tran = databaseContext.Database.BeginTransaction();

                await databaseContext.EpisodeSongListSyncStatusData.Where(a => a.EpisodeId == episodeId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);
                await databaseContext.EpisodeSongListItemData.Where(a => a.EpisodeId == episodeId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);

                tran.Commit();
            }).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(DeleteEpisodeSongListAsync), ex);

            throw;
        }

        Log($"{nameof(DeleteEpisodeSongListAsync)} End");
    }

    internal async Task InsertOrUpdateChannelSongListItemsAsync(int channelId, IList<ChannelSongListItemData> songs, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateChannelSongListItemsAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var now = DateTimeProvider.OffsetNow;
                foreach (var data in songs)
                {
                    data.ChannelId = channelId;
                    data.LatestUpdateTime = now;
                }

                using var tran = databaseContext.Database.BeginTransaction();

                //Delete existing
                await databaseContext.ChannelSongListItemData.Where(a => a.ChannelId == channelId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertAsync(songs, cancellationToken: cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertOrUpdateAsync(new List<ChannelSongListSyncStatusData>() {
                    new ChannelSongListSyncStatusData()
                    {
                        ChannelId = channelId,
                        LatestUpdateTime = now
                    }
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

                tran.Commit();
                
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateChannelSongListItemsAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(InsertOrUpdateChannelSongListItemsAsync)} End");
        }
    }

    public async Task<IList<ChannelSongListItemData>?> GetChannelsSongListItemsAsync(int channelId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ChannelSongListItemData
                        .Where(e => e.ChannelId == channelId)
                        .ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetChannelsSongListItemsAsync), ex);

            throw;
        }
    }

    public async Task<DateTimeOffset> GetLatestChannelSongListSyncTimeAsync(int channelId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var latest = databaseContext.ChannelSongListSyncStatusData
                        .Where(e => e.ChannelId == channelId)
                        .FirstOrDefault();

                if (latest is not null)
                {
                    return latest.LatestUpdateTime;
                }

                return DateTimeOffset.MinValue;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetLatestEpisodeSongListSyncTimeAsync), ex);

            throw;
        }
    }

    public async Task DeleteChannelSongListAsync(int channelId, CancellationToken cancellationToken)
    {
        Log($"{nameof(DeleteChannelSongListAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using var tran = databaseContext.Database.BeginTransaction();

                await databaseContext.ChannelSongListSyncStatusData.Where(a => a.ChannelId == channelId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);
                await databaseContext.ChannelSongListItemData.Where(a => a.ChannelId == channelId).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);

                tran.Commit();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(DeleteChannelSongListAsync), ex);

            throw;
        }

        Log($"{nameof(DeleteChannelSongListAsync)} End");
    }

    internal async Task InsertOrUpdateChannelStatusAsync(ChannelStatusData status, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateChannelStatusAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                status.LatestUpdateTime = DateTimeProvider.OffsetNow;

                await databaseContext.BulkInsertOrUpdateAsync(new List<ChannelStatusData>() { status }, cancellationToken: cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateChannelStatusAsync), ex);

            throw;
        }

        Log($"{nameof(InsertOrUpdateChannelStatusAsync)} End");
    }

    internal async Task InsertOrUpdateChannelStatusesAsync(IList<ChannelStatusData> statuses, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateChannelStatusesAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                List<ChannelStatusData> result = new List<ChannelStatusData>();

                var now = DateTimeProvider.OffsetNow;
                foreach (var status in statuses)
                {
                    status.LatestUpdateTime = now;

                    result.Add(status);
                }

                using var tran = databaseContext.Database.BeginTransaction();

                await databaseContext.ChannelStatusData.Where(a => true).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertOrUpdateAsync(statuses, cancellationToken: cancellationToken).ConfigureAwait(false);

                tran.Commit();

            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateChannelStatusesAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(InsertOrUpdateChannelStatusesAsync)} End");
        }
    }

    public async Task<ChannelStatusData?> GetChannelStatusDataAsync(int channelId, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ChannelStatusData
                        .Where(e => e.ChannelId == channelId)
                        .FirstOrDefault();
            
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetChannelStatusDataAsync), ex);

            throw;
        }
    }

    public async Task<IList<ChannelStatusData>?> GetChannelStatusesDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ChannelStatusData.ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetChannelStatusesDataAsync), ex);

            throw;
        }
    }


    internal async Task InsertOrUpdateScheduledEpisodeListItemDataAsync(int channelId, DateTimeOffset date, IList<ScheduledEpisodeListItemData> episodes, CancellationToken cancellationToken)
    {
        Log($"{nameof(InsertOrUpdateScheduledEpisodeListItemDataAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                var now = DateTimeProvider.OffsetNow;

                foreach (var data in episodes)
                {
                    data.ChannelId = channelId;
                    data.Date = date;
                }

                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using var tran = databaseContext.Database.BeginTransaction();

                //Delete existing
                await databaseContext.ScheduledEpisodeListItemData.Where(a => a.ChannelId == channelId && a.Date == date).BatchDeleteAsync(cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertAsync(episodes, cancellationToken: cancellationToken).ConfigureAwait(false);

                await databaseContext.BulkInsertOrUpdateAsync(new List<ScheduledEpisodeListSyncStatusData>() {
                    new ScheduledEpisodeListSyncStatusData()
                    {
                        ChannelId = channelId,
                        Date = date,
                        LatestUpdateTime = now
                    }
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

                tran.Commit();
                
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(InsertOrUpdateScheduledEpisodeListItemDataAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(InsertOrUpdateScheduledEpisodeListItemDataAsync)} End");
        }
    }

    public async Task<IList<ScheduledEpisodeListItemData>?> GetScheduledEpisodListItemDataItemsAsync(int channelId, DateTimeOffset date, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                return databaseContext.ScheduledEpisodeListItemData
                                    .Where(e => e.ChannelId == channelId && e.Date == date)
                                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetScheduledEpisodListItemDataItemsAsync), ex);

            throw;
        }
    }

    public async Task<DateTimeOffset> GetLatestScheduledEpisodListSyncTimeAsync(int channelId, DateTimeOffset date, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                var latest = databaseContext.ScheduledEpisodeListSyncStatusData
                                .Where(e => e.ChannelId == channelId && e.Date == date)
                                .FirstOrDefault();

                if (latest is not null)
                {
                    return latest.LatestUpdateTime;
                }

                return DateTimeOffset.MinValue;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(GetLatestEpisodeSongListSyncTimeAsync), ex);

            throw;
        }
    }

    public async Task DeleteObseleteEpisodesAsync(List<int> programsIdsToKeep, List<int> episodeIdsToKeep, CancellationToken cancellationToken)
    {
        Log($"{nameof(DeleteObseleteEpisodesAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                var obseleteTime = DateTimeProvider.OffsetNow.AddDays(-7);

                using var databaseContext = CacheDatabaseContextFactory.Create();

                using var tran = databaseContext.Database.BeginTransaction();

                var obseleteProgramIds = databaseContext.EpisodeListSyncStatusData
                            .Where(e => !programsIdsToKeep.Contains(e.ProgramId) && e.LatestUpdateTime < obseleteTime)
                            .Select(a => (int?)a.ProgramId)
                            .ToList();

                await databaseContext.EpisodeListSyncStatusData
                            .Where(e => obseleteProgramIds.Contains(e.ProgramId))
                            .BatchDeleteAsync(cancellationToken)
                            .ConfigureAwait(false);

                await databaseContext.EpisodeData
                            .Where(e => obseleteProgramIds.Contains(e.ProgramId)
                                            &&
                                            !episodeIdsToKeep.Contains(e.EpisodeId)
                                    )
                            .BatchDeleteAsync(cancellationToken)
                            .ConfigureAwait(false);

                tran.Commit();
                
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(DeleteObseleteEpisodesAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(DeleteObseleteEpisodesAsync)} End");
        }
    }

    public async Task DeleteObseleteSongListsAsync(CancellationToken cancellationToken)
    {
        Log($"{nameof(DeleteObseleteSongListsAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                var obseleteTime = DateTimeProvider.OffsetNow.AddDays(-7);

                using var databaseContext = CacheDatabaseContextFactory.Create();

                using (var tran = databaseContext.Database.BeginTransaction())
                {
                    await databaseContext.EpisodeSongListItemData
                        .Where(e => e.LatestUpdateTime < obseleteTime)
                        .BatchDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await databaseContext.EpisodeSongListSyncStatusData
                        .Where(e => e.LatestUpdateTime < obseleteTime)
                        .BatchDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);

                    tran.Commit();
                };

                using (var tran = databaseContext.Database.BeginTransaction())
                {
                    await databaseContext.ChannelSongListItemData
                        .Where(e => e.LatestUpdateTime < obseleteTime)
                        .BatchDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await databaseContext.ChannelSongListSyncStatusData
                        .Where(e => e.LatestUpdateTime < obseleteTime)
                        .BatchDeleteAsync(cancellationToken)
                        .ConfigureAwait(false);

                    tran.Commit();
                };
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(DeleteObseleteSongListsAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(DeleteObseleteSongListsAsync)} End");
        }
    }

    public async Task DeleteObseleteScheduledEpisodeListsAsync(CancellationToken cancellationToken)
    {
        Log($"{nameof(DeleteObseleteScheduledEpisodeListsAsync)} Start");

        try
        {
            await Task.Run(async () =>
            {
                var obseleteTime = DateTimeProvider.OffsetNow.AddDays(-7);

                using var databaseContext = CacheDatabaseContextFactory.Create();
                
                using (var tran = databaseContext.Database.BeginTransaction())
                {
                    await databaseContext.ScheduledEpisodeListSyncStatusData
                            .Where(e => e.Date < obseleteTime)
                            .BatchDeleteAsync(cancellationToken)
                            .ConfigureAwait(false);

                    await databaseContext.ScheduledEpisodeListItemData
                            .Where(e => e.Date < obseleteTime)
                            .BatchDeleteAsync(cancellationToken)
                            .ConfigureAwait(false);

                    tran.Commit();
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogError(nameof(DeleteObseleteScheduledEpisodeListsAsync), ex);

            throw;
        }
        finally
        {
            Log($"{nameof(DeleteObseleteScheduledEpisodeListsAsync)} End");
        }
    }

    #endregion
}
