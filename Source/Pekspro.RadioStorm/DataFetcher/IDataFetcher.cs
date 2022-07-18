namespace Pekspro.RadioStorm.DataFetcher
{
    public interface IDataFetcher
    {
        Task<ChannelData?> GetChannelAsync(int channelId, bool allowCache, CancellationToken cancellationToken = default);
        Task<IList<ChannelData>?> GetChannelsAsync(bool allowCache, CancellationToken cancellationToken = default);
        Task<ChannelStatusData?> GetChannelStatusAsync(int channelId, bool allowCache, CancellationToken cancellationToken = default);
        Task<IList<ChannelStatusData>?> GetChannelStatusesAsync(bool allowCache, CancellationToken cancellationToken = default);
        Task<IList<ChannelSongListItemData>?> GetChannelSongListAsync(int channelId, bool allowCache, CancellationToken cancellationToken = default);

        Task<ProgramData?> GetProgramAsync(int programId, bool allowCache, CancellationToken cancellationToken = default);
        Task<IList<ProgramData>?> GetProgramsAsync(bool allowCache, CancellationToken cancellationToken = default);


        Task<EpisodeData?> GetEpisodeAsync(int episodeId, bool allowCache, CancellationToken cancellationToken = default);
        Task<DataFetcher.GetListResult> GetEpisodesAsync(int programId, bool forceFullSynchronizing, bool allowCache = true, CancellationToken cancellationToken = default);
        Task<EpisodeData?> GetNextEpisodeAsync(int programId, DateTimeOffset currentPublishDate, CancellationToken cancellationToken = default);
        Task<EpisodeData?> GetPreviousEpisodeAsync(int programId, DateTimeOffset currentPublishDate, CancellationToken cancellationToken = default);
        Task<EpisodeData?> GetFirstMatchingEpisodeFromCacheAsync(int?[] validProgramId, int[] episodesToIgnore, bool orderAscending, CancellationToken cancellationToken = default);
        Task<IList<EpisodeSongListItemData>?> GetEpisodeSongListAsync(int episodeId, bool allowCache, CancellationToken cancellationToken = default);
        Task<IList<ScheduledEpisodeListItemData>?> GetScheduledEpisodeListAsync(int channelId, DateOnly swedishDate, bool allowCache, CancellationToken cancellationToken = default);
        Task<DataFetcher.SearchEpisodesResult> SearchEpisodesAsync(string searchString, bool fullSearch, CancellationToken cancellationToken = default);

        Task FetchAllProgramsAndAllEpisodesAsync(CancellationToken cancellationToken = default);
    }
}