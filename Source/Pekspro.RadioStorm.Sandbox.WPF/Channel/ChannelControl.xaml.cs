namespace Pekspro.RadioStorm.Sandbox.WPF.Channel
{
    /// <summary>
    /// Interaction logic for UserControlChannelItem.xaml
    /// </summary>
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
}
