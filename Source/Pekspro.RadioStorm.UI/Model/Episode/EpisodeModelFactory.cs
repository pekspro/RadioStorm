namespace Pekspro.RadioStorm.UI.Model.Episode;

public class EpisodeModelFactory : IEpisodeModelFactory
{
    #region Private properties
    
    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Constructor
    
    public EpisodeModelFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    #endregion

    #region Methods
    
    public EpisodeModel Create(EpisodeData episodeData)
    {
        var listenStateManager = ServiceProvider.GetRequiredService<IListenStateManager>();
        var audioManager = ServiceProvider.GetRequiredService<IAudioManager>();
        var messenger = ServiceProvider.GetRequiredService<IMessenger>();
        var localSettings = ServiceProvider.GetRequiredService<ILocalSettings>();
        var mainThreadRunner = ServiceProvider.GetRequiredService<IMainThreadRunner>();
        var downloadManager = ServiceProvider.GetRequiredService<IDownloadManager>();
        var dateTimeProvider = ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var weekdaynameHelper = ServiceProvider.GetRequiredService<IWeekdaynameHelper>();

        EpisodeModel model = new EpisodeModel
            (
                episodeData, 
                downloadManager, 
                listenStateManager, 
                audioManager, 
                localSettings, 
                mainThreadRunner, 
                dateTimeProvider,
                weekdaynameHelper,
                messenger
            );

        return model;
    }

    #endregion
}
