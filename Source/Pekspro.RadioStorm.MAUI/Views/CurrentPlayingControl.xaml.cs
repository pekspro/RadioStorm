using Pekspro.RadioStorm.UI.ViewModel.Player;

namespace Pekspro.RadioStorm.MAUI.Views;

public partial class CurrentPlayingControl
{
    public CurrentPlayingControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<CurrentPlayingViewModel>();

            BindingContext = s;

            s.OnNavigatedTo();
        }
    }

    private CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel) BindingContext;
}
