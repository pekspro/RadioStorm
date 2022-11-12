namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ChannelAlbumControl
{
    public ChannelAlbumControl()
    {
        InitializeComponent();
    }

    private ChannelModel ViewModel => (ChannelModel) BindingContext;

    private void AlbumItem_SizeChanged(object sender, EventArgs e)
    {
        LargeMediaButton.WidthRequest = Width - 16;
    }
}
