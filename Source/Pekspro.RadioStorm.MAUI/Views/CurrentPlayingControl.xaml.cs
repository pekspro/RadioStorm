namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class CurrentPlayingControl
{
    public CurrentPlayingControl()
    {
        InitializeComponent();

        if (ServiceProviderHelper.Current is not null)
        {
            var s = ServiceProviderHelper.GetRequiredService<CurrentPlayingViewModel>();

            BindingContext = s;

            s.OnNavigatedTo();
        }
    }

    private CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel) BindingContext;
}
