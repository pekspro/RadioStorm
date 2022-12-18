using Microsoft.Extensions.Logging;

namespace Pekspro.RadioStorm.DataFetcher;

public sealed class DataFetcher : IDataFetcher
{
    #region Private properties

    private const int PaginationSize = 500;
    private const int MaxPageCount = 20;
    private const int MaxEpisodeDownloadCount = PaginationSize * MaxPageCount;

    private CacheDatabaseManager CacheDatabase { get; }
    private IDtoConverter DtoConverter { get; }
    private IDateTimeProvider DateTimeProvider { get; }
    private ILogger Logger { get; }

    #endregion

    #region Constructor

    public DataFetcher
        (
            CacheDatabaseManager cacheDatabase, 
            IDtoConverter dtoConverter, 
            IDateTimeProvider dateTimeProvider,
            ILogger<DataFetcher> logger
        )
    {
        CacheDatabase = cacheDatabase;
        DtoConverter = dtoConverter;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;
    }

    #endregion

    #region Helpers

    private async Task<bool> RunWithClientAsync(Func<SrClient, Task> action)
    {
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                SrClient srClient = new SrClient(httpClient);

                await action.Invoke(srClient).ConfigureAwait(false);
            }

            return true;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {  
            Logger.LogError("Error when downloading: " + ex.Message);

            return false;
        }
    }

    private async Task<bool> RunWithPaginatedClientAsync(Func<SrClient, int, Task<int>> action, int maxPageCount = MaxPageCount)
    {
        try
        {
            using (HttpClient httpClient = new HttpClient())
            {
                SrClient srClient = new SrClient(httpClient);
                int page = 1;

                int totalPages = await action.Invoke(srClient, page).ConfigureAwait(false);

                while (page < maxPageCount && page < totalPages)
                {
                    page++;

                    totalPages = await action.Invoke(srClient, page).ConfigureAwait(false);
                }
            }

            return true;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error when downloading.");

            return false;
        }
    }

    #endregion

    #region Channel

    public async Task<ChannelData?> GetChannelAsync(int channelId, bool allowCache, CancellationToken cancellationToken = default)
    {
        ChannelData? channelData = null;

        if (allowCache)
        {
            channelData = await CacheDatabase.GetChannelAsync(channelId, cancellationToken).ConfigureAwait(false);
        }

        if (channelData is null || CacheTime.IsTimeOut(channelData.LatestUpdateTime, CacheTime.ChannelDataCacheTime))
        {
            bool res = await RunWithClientAsync(async (a) =>
            {
                var response = await a.GetChannelAsync(Format.Json, channelId, null, null, cancellationToken).ConfigureAwait(false);

                channelData = DtoConverter.Convert(response.Channel);
            }).ConfigureAwait(false);

            if (res && channelData is not null)
            {
                await CacheDatabase.InsertOrUpdateChannelAsync(channelData, cancellationToken).ConfigureAwait(false);
            }
        }

        return channelData;
    }
    
    public async Task<IList<ChannelData>?> GetChannelsAsync(bool allowCache, CancellationToken cancellationToken = default)
    {
        DateTimeOffset latestUpdateTime = DateTimeOffset.MinValue;

        IList<ChannelData>? dbChannelData = null;

        if (allowCache)
        {
            latestUpdateTime = await CacheDatabase.GetLatestListSyncTimeAsync(ListSyncStatusData.ListType.Channels, cancellationToken).ConfigureAwait(false);

            if (!CacheTime.IsTimeOut(latestUpdateTime, CacheTime.ChannelListCacheTime))
            {
                dbChannelData = await CacheDatabase.GetChannelsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (dbChannelData is null)
        {
            List<ChannelData> channels = new List<ChannelData>();

            bool res = await RunWithPaginatedClientAsync(async (a, page) =>
            {
                var response = await a.GetChannelsAsync(Format.Json, page, PaginationSize, null, null, null, cancellationToken).ConfigureAwait(false);

                channels.AddRange(DtoConverter.Convert(response.Channels));

                return response.Pagination.Totalpages;
            }).ConfigureAwait(false);

            if (res)
            {
                await CacheDatabase.InsertOrUpdateChannelsAsync(channels, cancellationToken).ConfigureAwait(false);

                dbChannelData = channels;
            }
        }

        return dbChannelData;
    }

    public async Task<ChannelStatusData?> GetChannelStatusAsync(int channelId, bool allowCache, CancellationToken cancellationToken = default)
    {
        ChannelStatusData? channelStatus = null;

        if (allowCache)
        {
            channelStatus = await CacheDatabase.GetChannelStatusDataAsync(channelId, cancellationToken).ConfigureAwait(false);
        }

        if (channelStatus is null || CacheTime.IsTimeOut(channelStatus.LatestUpdateTime, CacheTime.ChannelStatusCacheTime))
        {
            bool res = await RunWithClientAsync(async (a) =>
            {
                var response = await a.GetEpisodesRightNowForChannelAsync(Format.Json, channelId, cancellationToken).ConfigureAwait(false);

                channelStatus = DtoConverter.Convert(response.Channel);
            }).ConfigureAwait(false);

            if (res && channelStatus is not null)
            {
                await CacheDatabase.InsertOrUpdateChannelStatusAsync(channelStatus, cancellationToken).ConfigureAwait(false);
            }
        }

        return channelStatus;
    }

    public async Task<IList<ChannelStatusData>?> GetChannelStatusesAsync(bool allowCache, CancellationToken cancellationToken = default)
    {
        IList<ChannelStatusData>? channelStatuses = null;

        if (allowCache)
        {
            channelStatuses = await CacheDatabase.GetChannelStatusesDataAsync(cancellationToken).ConfigureAwait(false);
        }

        var latestUpdateTime = DateTimeOffset.MinValue;

        if (channelStatuses is not null && channelStatuses.Any())
        {
            latestUpdateTime = channelStatuses.Min(a => a.LatestUpdateTime);
        }

        if (channelStatuses is null || channelStatuses.Count <= 0 || CacheTime.IsTimeOut(latestUpdateTime, CacheTime.ChannelStatusCacheTime))
        {
            List<ChannelStatusData> channelStats = new List<ChannelStatusData>();

            bool res = await RunWithPaginatedClientAsync(async (a, page) =>
            {
                var response = await a.GetEpisodesRightNowAllChannelsAsync(Format.Json, page, PaginationSize, cancellationToken).ConfigureAwait(false);

                channelStats.AddRange(DtoConverter.Convert(response.Channels));

                return response.Pagination.Totalpages;
            }).ConfigureAwait(false);

            if (res)
            {
                await CacheDatabase.InsertOrUpdateChannelStatusesAsync(channelStats, cancellationToken).ConfigureAwait(false);

                channelStatuses = channelStats;
            }
        }

        return channelStatuses;
    }

    public async Task<IList<ChannelSongListItemData>?> GetChannelSongListAsync(int channelId, bool allowCache, CancellationToken cancellationToken = default)
    {
        DateTimeOffset latestUpdateTime = DateTimeOffset.MinValue;
        IList<ChannelSongListItemData>? songList = null;

        if (allowCache)
        {
            latestUpdateTime = await CacheDatabase.GetLatestChannelSongListSyncTimeAsync(channelId, cancellationToken).ConfigureAwait(false);

            if (!CacheTime.IsTimeOut(latestUpdateTime, CacheTime.ChannelSongListCacheTime))
            {
                songList = await CacheDatabase.GetChannelsSongListItemsAsync(channelId, cancellationToken).ConfigureAwait(false);
            }
        };

        if (songList is null)
        {
            bool res = await RunWithClientAsync(async (a) =>
            {
                var response = await a.GetPlaylistByChannelAsync(Format.Json, MaxEpisodeDownloadCount, channelId, DateTimeOffset.UtcNow.Date.AddDays(-1), DateTimeOffset.UtcNow.Date.AddDays(1), cancellationToken).ConfigureAwait(false);

                songList = DtoConverter.ConvertToChannelSongs(channelId, response.Song);
            }).ConfigureAwait(false);

            if (res && songList is not null)
            {
                await CacheDatabase.InsertOrUpdateChannelSongListItemsAsync(channelId, songList, cancellationToken).ConfigureAwait(false);
            }
        }

        return songList;
    }

    #endregion

    #region Program

    public async Task<ProgramData?> GetProgramAsync(int programId, bool allowCache, CancellationToken cancellationToken = default)
    {
        ProgramData? programData = null;

        if (allowCache)
        {
            programData = await CacheDatabase.GetProgramAsync(programId, cancellationToken).ConfigureAwait(false);
        }

        if (programData is null || CacheTime.IsTimeOut(programData.LatestUpdateTime, CacheTime.ProgramDataCacheTime))
        {
            bool res = await RunWithClientAsync(async (a) =>
            {
                var response = await a.GetProgramAsync(Format.Json, programId, cancellationToken).ConfigureAwait(false);

                programData = DtoConverter.Convert(response.Program);

            }).ConfigureAwait(false);

            if (res && programData is not null)
            {
                await CacheDatabase.InsertOrUpdateProgramAsync(programData, cancellationToken).ConfigureAwait(false);
            }
        }

        return programData;
    }

    public async Task<IList<ProgramData>?> GetProgramsAsync(bool allowCache, CancellationToken cancellationToken = default)
    {
        DateTimeOffset latestUpdateTime = DateTimeOffset.MinValue;
        IList<ProgramData>? dbProgramData = null;

        if (allowCache)
        {
          latestUpdateTime = await CacheDatabase.GetLatestListSyncTimeAsync(ListSyncStatusData.ListType.Programs, cancellationToken).ConfigureAwait(false);

            if (!CacheTime.IsTimeOut(latestUpdateTime, CacheTime.ProgramListCacheTime))
            {
                dbProgramData = await CacheDatabase.GetProgramsAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (dbProgramData is null)
        {
            List<ProgramData> programs = new List<ProgramData>();

            bool res = await RunWithPaginatedClientAsync(async (a, page) =>
            {
                var response = await a.GetProgramsAsync(Format.Json, page, PaginationSize, null, null, cancellationToken).ConfigureAwait(false);

                programs.AddRange(DtoConverter.Convert(response.Programs));

                return response.Pagination.Totalpages;
            }).ConfigureAwait(false);

            if (res)
            {
                await CacheDatabase.InsertOrUpdateProgramsAsync(programs, cancellationToken).ConfigureAwait(false);

                dbProgramData = programs;
            }
        }

        return dbProgramData;
    }

    #endregion

    #region Episode

    public async Task<EpisodeData?> GetEpisodeAsync(int episodeId, bool allowCache, CancellationToken cancellationToken = default)
    {
        EpisodeData? episodeData = null;

        if (allowCache)
        {
            episodeData = await CacheDatabase.GetEpisodeAsync(episodeId, cancellationToken).ConfigureAwait(false);
        }

        episodeData = await CheckAndUpdateEpisodeData(episodeId, episodeData, cancellationToken).ConfigureAwait(false);

        return episodeData;
    }

    public enum GetListResultEnum { Full, IncrementalUpdate, GotSomePart, Error }

    public sealed class GetListResult
    {
        public GetListResultEnum Result { get; set; }
        public EpisodeData[]? Episodes { get; set; }
        public DateTimeOffset LatestFullSyncronizingTime { get; set; }
        public int IncrementalUpdateCount { get; set; }
    }

    public async Task<GetListResult> GetEpisodesAsync(int programId, bool forceFullSynchronizing, bool allowCache = true, CancellationToken cancellationToken = default)
    {
        bool needsFullList = false;
        /*var episodesSortOrder = ServiceLocator.Current.GetInstance<EpisodesSortOrderManager>();

			//If sort ascending, download all is needed since the api doesn't support sort by publishdate.
			if (episodesSortOrder is not null && episodesSortOrder.IsFavorite(programId))
			{
				needsFullList = true;
			}
			*/

        EpisodeListSyncStatusData? currentSyncStatus = await CacheDatabase.GetEpisodeListSyncStatusAsync(programId, cancellationToken).ConfigureAwait(false);

        if (!forceFullSynchronizing)
        {
            if (currentSyncStatus is not null &&
                allowCache &&
                !CacheTime.IsTimeOut(currentSyncStatus.LatestUpdateTime, CacheTime.EpisodeListCacheTime) &&
                (!needsFullList || currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.FullySynchronized || currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.IncrementallyUpdated)
            )
            {
                var cachedData = await CacheDatabase.GetEpisodesAsync(programId, cancellationToken).ConfigureAwait(false);

                if (cachedData?.Count > 0)
                {
                    GetListResultEnum r = GetListResultEnum.GotSomePart;
                    if (currentSyncStatus is not null)
                    {
                        if (currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.FullySynchronized)
                        {
                            r = GetListResultEnum.Full;
                        }
                        else if (currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.IncrementallyUpdated)
                        {
                            r = GetListResultEnum.IncrementalUpdate;
                        }
                    }

                    await UpdateEpisodeDataThatHasExpired(cachedData, cancellationToken).ConfigureAwait(false);

                    return new GetListResult()
                    {
                        Episodes = cachedData.ToArray(),
                        Result = r,
                        IncrementalUpdateCount = currentSyncStatus!.IncrementallyUpdateCount,
                        LatestFullSyncronizingTime = currentSyncStatus.LatestFullSynchronizingTime
                    };
                }
            }
        }

        //If we need full list and don't have full list in cache, force full update.
        if (needsFullList)
        {
            if (currentSyncStatus is null)
            {
                forceFullSynchronizing = true;
            }
            else if (currentSyncStatus.Status != EpisodeListSyncStatusData.SyncStatus.FullySynchronized && currentSyncStatus.Status != EpisodeListSyncStatusData.SyncStatus.IncrementallyUpdated)
            {
                forceFullSynchronizing = true;
            }
        }


        var latestEpisode = await CacheDatabase.GetLatestEpisodeAsync(programId, cancellationToken).ConfigureAwait(false);

        List<EpisodeData> episodes = new List<EpisodeData>();
        GetListResultEnum listResultAfterUpdate = GetListResultEnum.GotSomePart;

        int pageSize = PaginationSize;
        int maxPageCount = MaxPageCount;

        if (!forceFullSynchronizing)
        {
            pageSize = 50;
            maxPageCount = 1;
        }

        bool res = await RunWithPaginatedClientAsync(async (a, page) =>
        {
            var response = await a.GetEpisodesAsync(Format.Json, page, pageSize, programId, null, null, null, cancellationToken).ConfigureAwait(false);

            episodes.AddRange(DtoConverter.Convert(response.Episodes));

            if (response.Pagination.Page >= response.Pagination.Totalpages)
            {
                listResultAfterUpdate = GetListResultEnum.Full;
            }

            return response.Pagination.Totalpages;
        }, maxPageCount).ConfigureAwait(false);

        if (res)
        {
            if (forceFullSynchronizing)
            {
                listResultAfterUpdate = GetListResultEnum.Full;
            }

            if (listResultAfterUpdate == GetListResultEnum.GotSomePart && currentSyncStatus is not null)
            {
                if (currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.FullySynchronized || currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.IncrementallyUpdated)
                {
                    if (latestEpisode is not null)
                    {
                        //If latest known episode from database is in downloaded list, assume we are up to date
                        if (episodes.Any(e => e.EpisodeId == latestEpisode.EpisodeId))
                        {
                            listResultAfterUpdate = GetListResultEnum.IncrementalUpdate;
                        }
                    }
                }
            }

            EpisodeListSyncStatusData listStatus = new EpisodeListSyncStatusData();
            listStatus.ProgramId = programId;

            if (listResultAfterUpdate == GetListResultEnum.Full)
            {
                listStatus.Status = EpisodeListSyncStatusData.SyncStatus.FullySynchronized;
            }
            else if (listResultAfterUpdate == GetListResultEnum.GotSomePart)
            {
                listStatus.Status = EpisodeListSyncStatusData.SyncStatus.HasSomeData;
            }
            else
            {
                listStatus.Status = EpisodeListSyncStatusData.SyncStatus.IncrementallyUpdated;

                if (currentSyncStatus is not null)
                {
                    listStatus.IncrementallyUpdateCount = currentSyncStatus.IncrementallyUpdateCount + 1;
                    listStatus.LatestFullSynchronizingTime = currentSyncStatus.LatestFullSynchronizingTime;
                }
            }

            await CacheDatabase.InsertOrUpdateEpisodesAsync(programId, episodes, listStatus, cancellationToken).ConfigureAwait(false);

            var dbDataAfterUpdate = await CacheDatabase.GetEpisodesAsync(programId, cancellationToken).ConfigureAwait(false);

            if (dbDataAfterUpdate is null || dbDataAfterUpdate.Count < episodes.Count)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                // Create list from memory instead.
                dbDataAfterUpdate = episodes;
            }

            await UpdateEpisodeDataThatHasExpired(dbDataAfterUpdate, cancellationToken).ConfigureAwait(false);

            return new GetListResult()
            {
                Episodes = dbDataAfterUpdate.ToArray(),
                Result = listResultAfterUpdate,
                IncrementalUpdateCount = listStatus.IncrementallyUpdateCount,
                LatestFullSyncronizingTime = listStatus.LatestFullSynchronizingTime
            };
        }

        var dbData = await CacheDatabase.GetEpisodesAsync(programId, cancellationToken).ConfigureAwait(false);

        if (dbData is null || dbData.Count <= 0)
        {
            return new GetListResult()
            {
                Episodes = null,
                Result = GetListResultEnum.Error
            };
        }

        GetListResultEnum listResult = GetListResultEnum.GotSomePart;
        if (currentSyncStatus is not null)
        {
            if (currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.FullySynchronized)
            {
                listResult = GetListResultEnum.Full;
            }
            else if (currentSyncStatus.Status == EpisodeListSyncStatusData.SyncStatus.IncrementallyUpdated)
            {
                listResult = GetListResultEnum.IncrementalUpdate;
            }
        }

        await UpdateEpisodeDataThatHasExpired(dbData, cancellationToken).ConfigureAwait(false);

        return new GetListResult()
        {
            Episodes = dbData.ToArray(),
            Result = listResult,
            IncrementalUpdateCount = currentSyncStatus?.IncrementallyUpdateCount ?? 0,
            LatestFullSyncronizingTime = currentSyncStatus?.LatestFullSynchronizingTime ?? DateTimeOffset.MinValue
        };
    }

    public async Task<EpisodeData?> GetNextEpisodeAsync(int programId, DateTimeOffset currentPublishDate, CancellationToken cancellationToken = default)
    {
        var dbEpisodeData = await CacheDatabase.GetNextEpisodeAsync(programId, currentPublishDate, cancellationToken).ConfigureAwait(false);

        if (dbEpisodeData is not null)
        {
            dbEpisodeData = await CheckAndUpdateEpisodeData(dbEpisodeData.EpisodeId, dbEpisodeData, cancellationToken).ConfigureAwait(false);

            return dbEpisodeData;
        }

        return dbEpisodeData;
    }

    public async Task<EpisodeData?> GetPreviousEpisodeAsync(int programId, DateTimeOffset currentPublishDate, CancellationToken cancellationToken = default)
    {
        var dbEpisodeData = await CacheDatabase.GetPreviousEpisodeAsync(programId, currentPublishDate, cancellationToken).ConfigureAwait(false);

        if (dbEpisodeData is not null)
        {
            dbEpisodeData = await CheckAndUpdateEpisodeData(dbEpisodeData.EpisodeId, dbEpisodeData, cancellationToken).ConfigureAwait(false);

            return dbEpisodeData;
        }

        return dbEpisodeData;
    }

    public Task<EpisodeData?> GetFirstMatchingEpisodeFromCacheAsync
    (
        int?[] validProgramId,
        int[] episodesToIgnore,
        bool orderAscending,
        CancellationToken cancellationToken = default
    )
    {
        return CacheDatabase.GetFirstMatchingEpisodeAsync(validProgramId, episodesToIgnore, orderAscending, cancellationToken);
    }

    public async Task<IList<EpisodeSongListItemData>?> GetEpisodeSongListAsync(int episodeId, bool allowCache, CancellationToken cancellationToken = default)
    {
        DateTimeOffset latestUpdateTime = DateTimeOffset.MinValue;
        IList<EpisodeSongListItemData>? songList = null;

        if (allowCache)
        {
            latestUpdateTime = await CacheDatabase.GetLatestEpisodeSongListSyncTimeAsync(episodeId, cancellationToken).ConfigureAwait(false);

            if (!CacheTime.IsTimeOut(latestUpdateTime, CacheTime.EpisodeSongListCacheTime))
            {
                songList = await CacheDatabase.GetEpisodesSongListItemsAsync(episodeId, cancellationToken).ConfigureAwait(false);
            }
        }

        if (songList is null)
        {
            bool res = await RunWithClientAsync(async (a) =>
            {
                var response = await a.GetPlaylistByEpisodeAsync(Format.Json, episodeId, cancellationToken).ConfigureAwait(false);

                songList = DtoConverter.ConvertToEpisodeSongs(episodeId, response.Song);
            }).ConfigureAwait(false);

            if (res && songList is not null)
            {
                await CacheDatabase.InsertOrUpdateEpisodesSongListItemsAsync(episodeId, songList, cancellationToken).ConfigureAwait(false);
            }
        }

        return songList;
    }

    public async Task<IList<ScheduledEpisodeListItemData>?> GetScheduledEpisodeListAsync(int channelId, DateOnly swedishDate, bool allowCache, CancellationToken cancellationToken = default)
    {
        DateTimeOffset latestUpdateTime = DateTimeOffset.MinValue;
        IList<ScheduledEpisodeListItemData>? episodeList = null;

        DateTimeOffset date = new DateTimeOffset(swedishDate.ToDateTime(TimeOnly.MinValue));

        if (allowCache)
        {
            latestUpdateTime = await CacheDatabase.GetLatestScheduledEpisodListSyncTimeAsync(channelId, date, cancellationToken).ConfigureAwait(false);

            if (!CacheTime.IsTimeOut(latestUpdateTime, CacheTime.ScheduledEpisodeDataCacheTime))
            {
                episodeList = await CacheDatabase.GetScheduledEpisodListItemDataItemsAsync(channelId, date, cancellationToken).ConfigureAwait(false);
            }
        }

        if (episodeList is null)
        {
            List<ScheduledEpisodeListItemData> episodes = new List<ScheduledEpisodeListItemData>();

            bool res = await RunWithPaginatedClientAsync(async (a, page) =>
            {
                var response = await a.GetScheduledEpisodesForChannelAsync(Format.Json, page, PaginationSize, channelId, date, cancellationToken).ConfigureAwait(false);

                episodes.AddRange(DtoConverter.Convert(channelId, date, response.Schedule));

                return response.Pagination.Totalpages;
            }).ConfigureAwait(false);

            if (res)
            {
                await CacheDatabase.InsertOrUpdateScheduledEpisodeListItemDataAsync(channelId, date, episodes, cancellationToken).ConfigureAwait(false);

                episodeList = episodes;
            }
        }

        return episodeList;
    }

    private async Task<EpisodeData?> CheckAndUpdateEpisodeData(int episodeId, EpisodeData? dbEpisodeData, CancellationToken cancellationToken = default)
    {
        if (dbEpisodeData is null || CacheTime.IsTimeOut(dbEpisodeData.LatestUpdateTime, CacheTime.EpisodeDataCacheTime) ||
                        dbEpisodeData.AudioStreamWithMusicExpireDate.HasValue && dbEpisodeData.AudioStreamWithMusicExpireDate.Value < DateTimeProvider.OffsetNow)
        {
            bool res = await RunWithClientAsync(async (a) =>
            {
                var response = await a.GetEpisodeAsync(Format.Json, episodeId, null, cancellationToken).ConfigureAwait(false);

                dbEpisodeData = DtoConverter.Convert(response.Episode);
            }).ConfigureAwait(false);

            if (res && dbEpisodeData is not null)
            {
                await CacheDatabase.InsertOrUpdateEpisodeAsync(dbEpisodeData, cancellationToken).ConfigureAwait(false);
            }
        }

        return dbEpisodeData;
    }

    private async Task UpdateEpisodeDataThatHasExpired(IList<EpisodeData> episodes, CancellationToken cancellationToken)
    {
        int updateCount = 0;

        for (int i = 0; i < episodes.Count && updateCount < 10; i++)
        {
            var dbEpisodeData = episodes[i];

            //Don't update if just updated
            if (Math.Abs((dbEpisodeData.LatestUpdateTime - DateTimeProvider.OffsetNow).TotalSeconds) < 5 * 60)
            {
                continue;
            }

            if (dbEpisodeData.AudioStreamWithMusicExpireDate.HasValue && dbEpisodeData.AudioStreamWithMusicExpireDate.Value < DateTimeProvider.OffsetNow)
            {
                updateCount++;

                await RunWithClientAsync(async (a) =>
                {
                    var response = await a.GetEpisodeAsync(Format.Json, dbEpisodeData.EpisodeId, null, cancellationToken).ConfigureAwait(false);

                    var episodeData = DtoConverter.Convert(response.Episode);

                    await CacheDatabase.InsertOrUpdateEpisodeAsync(episodeData, cancellationToken).ConfigureAwait(false);

                    episodes[i] = episodeData;
                }).ConfigureAwait(false);
            }
        }
    }

    #endregion

    #region Search

    private List<SearchEpisodesResult> SearchEpisodesResultCache = new List<SearchEpisodesResult>();

    private const int SearchEpisodesResultCacheSize = 25;

    public sealed class SearchEpisodesResult
    {
        public SearchEpisodesResult(string searchString)
        {
            SearchString = searchString;
        }

        public string SearchString { get; set; }
        public GetListResultEnum Result { get; set; }
        public EpisodeData[]? Episodes { get; set; }
        public bool IsFullSearch { get; set; }
    }

    public async Task<SearchEpisodesResult> SearchEpisodesAsync(string searchString, bool fullSearch, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return new SearchEpisodesResult(searchString)
            {
                Result = GetListResultEnum.Error
            };
        }

        searchString = searchString.Trim();

        var cachedSearch = SearchEpisodesResultCache.FirstOrDefault(a =>
                                a.SearchString == searchString &&
                                (
                                    !fullSearch || a.IsFullSearch
                                )
                            );

        if (cachedSearch is not null)
        {
            SearchEpisodesResultCache.Remove(cachedSearch);
            SearchEpisodesResultCache.Insert(0, cachedSearch);

            return cachedSearch;
        }

        List<EpisodeData> channels = new List<EpisodeData>();
        SearchEpisodesResult ret = new SearchEpisodesResult(searchString)
        {
            Episodes = null,
            Result = GetListResultEnum.Error
        };
        bool allItemsFound = false;
        const int maxSearchHits = 25;

        bool res = await RunWithPaginatedClientAsync(async (a, page) =>
        {
            var response = await a.SearchEpisodesAsync(Format.Json, page, maxSearchHits, searchString, null, cancellationToken).ConfigureAwait(false);

            channels.AddRange(DtoConverter.Convert(response.Episodes));

            if (response.Pagination.Page >= response.Pagination.Totalpages)
            {
                allItemsFound = true;
            }

            return response.Pagination.Totalpages;
        }, fullSearch ? 500 / maxSearchHits : 100 / maxSearchHits).ConfigureAwait(false);

        if (res)
        {
            if (channels.Any())
            {
                await CacheDatabase.InsertOrUpdateEpisodesAsync(channels, cancellationToken).ConfigureAwait(false);
            }

            ret = new SearchEpisodesResult(searchString)
            {
                Episodes = channels.ToArray(),
                Result = allItemsFound ? GetListResultEnum.Full : GetListResultEnum.GotSomePart,
                IsFullSearch = fullSearch
            };

            SearchEpisodesResultCache.Insert(0, ret);

            while (SearchEpisodesResultCache.Count > SearchEpisodesResultCacheSize)
            {
                SearchEpisodesResultCache.RemoveAt(SearchEpisodesResultCache.Count - 1);
            }
        }

        return ret;
    }

    #endregion

    #region Test

    public async Task FetchAllProgramsAndAllEpisodesAsync(CancellationToken cancellationToken = default)
    {
        var allPrograms = await GetProgramsAsync(true, cancellationToken).ConfigureAwait(false);
        if (allPrograms is null)
        {
            return;
        }

        foreach (var program in allPrograms)
        {
            Logger.LogInformation("Fetching episodes from: " + program.ProgramId + " " + program.Name);
            var allEpisodes = await GetEpisodesAsync(program.ProgramId, true, true, cancellationToken).ConfigureAwait(false);
            if (allEpisodes is null)
            {
                Logger.LogInformation("Got no episodes :-(");
            }
            else
            {
                Logger.LogInformation($"Got {allEpisodes.Episodes?.Length} episodes.");
            }
        }
    }

    #endregion
}