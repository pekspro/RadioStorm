namespace Pekspro.RadioStorm.UI.Utilities;

public interface IChannelRefreshHelper : IDisposable
{
    IMainThreadTimer ChannelStatusTimer { get; }
    IMainThreadTimer ChannelProgressTimer { get; }
    DownloadViewModel? ViewModel { get; set; }

    void Stop();

    void RefreshChannelProgress(IEnumerable<FavoriteBaseModel?>? models);

    Task RefreshChannelStatusAsync(IDataFetcher dataFetcher, IEnumerable<FavoriteBaseModel?>? models, bool allowCache, CancellationToken cancellationToken);

    void SetupStatusRefreshTimer(IEnumerable<FavoriteBaseModel?>? models);
}
