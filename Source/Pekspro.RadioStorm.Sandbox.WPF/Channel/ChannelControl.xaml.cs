namespace Pekspro.RadioStorm.Sandbox.WPF.Channel;

public sealed partial class ChannelControl : UserControl
{
    public ChannelControl()
    {
        InitializeComponent();
    }

    private ChannelModel ViewModel
    {
        get
        {
            var vm = DataContext as ChannelModel;

            return vm;
        }
    }
}
