namespace Pekspro.RadioStorm.Sandbox.WPF.Channel;

public partial class ChannelControl : UserControl
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
