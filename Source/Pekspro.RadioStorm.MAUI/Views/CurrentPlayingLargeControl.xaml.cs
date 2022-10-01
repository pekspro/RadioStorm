namespace Pekspro.RadioStorm.MAUI.Views;

public partial class CurrentPlayingLargeControl : ContentView
{
    public CurrentPlayingLargeControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<CurrentPlayingViewModel>();

            BindingContext = s;
        }
    }

    private CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel)BindingContext;
}