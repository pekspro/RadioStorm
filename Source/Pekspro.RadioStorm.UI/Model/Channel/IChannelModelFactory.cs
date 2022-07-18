namespace Pekspro.RadioStorm.UI.Model.Channel;

public interface IChannelModelFactory
{
    ChannelModel Create(ChannelData channelData);
}