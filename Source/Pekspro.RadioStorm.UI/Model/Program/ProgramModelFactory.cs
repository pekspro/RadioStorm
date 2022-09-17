namespace Pekspro.RadioStorm.UI.Model.Program;

public sealed class ProgramModelFactory : IProgramModelFactory
{
    #region Private properites
    
    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Constructor

    public ProgramModelFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    #endregion

    #region Methods

    public ProgramModel Create(ProgramData programData)
    {
        var programFavoriteList = ServiceProvider.GetRequiredService<IProgramFavoriteList>();
        var messenger = ServiceProvider.GetRequiredService<IMessenger>();
        var uriLauncher = ServiceProvider.GetRequiredService<IUriLauncher>();
        var episodesSortOrderManager = ServiceProvider.GetRequiredService<IEpisodesSortOrderManager>();
        var logger = ServiceProvider.GetRequiredService<ILogger<ProgramModel>>();

        ProgramModel model = new ProgramModel(programFavoriteList, messenger, uriLauncher, episodesSortOrderManager, programData, logger);

        return model;
    }

    #endregion
}
