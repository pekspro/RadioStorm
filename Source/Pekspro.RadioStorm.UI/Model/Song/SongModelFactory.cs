namespace Pekspro.RadioStorm.UI.Model.Song;

public sealed class SongModelFactory : ISongModelFactory
{
    #region Private properties
    
    private IServiceProvider ServiceProvider { get; }

    #endregion

    #region Constructor
    
    public SongModelFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    #endregion

    #region Methods
    
    public SongModel Create(SongListItemData songListItemData, EpisodeData? episodeData)
    {
        var audioManager = ServiceProvider.GetRequiredService<IAudioManager>();
        var weekdaynameHelper = ServiceProvider.GetRequiredService<IWeekdaynameHelper>();
        var logger = ServiceProvider.GetRequiredService<ILogger<SongModel>>();

        var model = new SongModel(songListItemData, episodeData, audioManager, weekdaynameHelper, logger);

        return model;
    }

    #endregion
}
