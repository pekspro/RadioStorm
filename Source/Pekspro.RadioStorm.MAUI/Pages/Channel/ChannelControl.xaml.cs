namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ChannelControl
{
    public ChannelControl()
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(this);
    }

    private ChannelModel ViewModel => (ChannelModel) BindingContext;
}
