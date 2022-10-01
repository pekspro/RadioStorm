namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ChannelAlbumControl
{
    public ChannelAlbumControl()
    {
        InitializeComponent();
    }

    private ChannelModel ViewModel => (ChannelModel) BindingContext;
}
