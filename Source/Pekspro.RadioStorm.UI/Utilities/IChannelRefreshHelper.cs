namespace Pekspro.RadioStorm.UI.Utilities;

public interface IChannelRefreshHelper : IDisposable
{
    IMainThreadTimer ChannelStatusTimer { get; }
    IMainThreadTimer ChannelProgressTimer { get; }
    DownloadViewModel? ViewModel { get; set; }

    void Stop();

    void RefreshChannelProgress(FavoriteBaseModel? models);
    
    void RefreshChannelProgress(IEnumerable<FavoriteBaseModel?>? models);

    Task RefreshChannelStatusAsync(IDataFetcher dataFetcher, FavoriteBaseModel? model, RefreshSettings refreshSettings, bool setupTimer, CancellationToken cancellationToken);
    
    Task RefreshChannelStatusAsync(IDataFetcher dataFetcher, IEnumerable<FavoriteBaseModel?>? models, RefreshSettings refreshSettings, bool setupTimer, CancellationToken cancellationToken);

    void SetupStatusRefreshTimer(IEnumerable<FavoriteBaseModel?>? models);
}
