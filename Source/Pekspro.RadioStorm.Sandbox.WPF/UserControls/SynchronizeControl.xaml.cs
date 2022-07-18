namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for SynchronizeControl.xaml
    /// </summary>
    public partial class SynchronizeControl : UserControl
    {
        public SynchronizeControl()
        {
            InitializeComponent();

            if (App.ServiceProvider is not null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<SynchronizingViewModel>();
            }
        }

        protected SynchronizingViewModel ViewModel => (SynchronizingViewModel)DataContext;
    }
}
