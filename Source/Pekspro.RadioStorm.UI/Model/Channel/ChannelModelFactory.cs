namespace Pekspro.RadioStorm.UI.Model.Channel;

public sealed class ChannelModelFactory : IChannelModelFactory
{
    #region Private properties
    
    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Constructor
    
    public ChannelModelFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    #endregion

    #region Methods

    public ChannelModel Create(ChannelData channelData)
    {
        var channelFavoriteList = ServiceProvider.GetRequiredService<IChannelFavoriteList>();
        var messenger = ServiceProvider.GetRequiredService<IMessenger>();
        var audioManager = ServiceProvider.GetRequiredService<IAudioManager>();
        var uriLauncher = ServiceProvider.GetRequiredService<IUriLauncher>();
        var logger = ServiceProvider.GetRequiredService<ILogger<ChannelModel>>();
        var dateTimeProvider = ServiceProvider.GetRequiredService<IDateTimeProvider>();

        ChannelModel model = new ChannelModel(channelFavoriteList, uriLauncher, messenger, audioManager, dateTimeProvider, channelData, logger);

        return model;
    }

    #endregion
}
