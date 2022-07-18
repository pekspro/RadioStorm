namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for CurrentPlayingControl.xaml
    /// </summary>
    public partial class CurrentPlayingControl : UserControl
    {
        public CurrentPlayingControl()
        {
            InitializeComponent();

            if (App.ServiceProvider is not null)
            {
                DataContext = App.ServiceProvider.GetRequiredService<CurrentPlayingViewModel>();

                ViewModel.OnNavigatedTo();
            }
        }

        protected CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel)DataContext;
    }
}
