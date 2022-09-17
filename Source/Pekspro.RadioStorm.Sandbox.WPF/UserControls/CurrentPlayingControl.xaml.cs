namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

public sealed partial class CurrentPlayingControl : UserControl
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

    private CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel)DataContext;
}
