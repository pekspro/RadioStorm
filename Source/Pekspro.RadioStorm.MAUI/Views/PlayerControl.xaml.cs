namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class PlayerControl
{
    public PlayerControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<PlayerViewModel>();

            BindingContext = s;
        }
    }

    private PlayerViewModel ViewModel => (PlayerViewModel) BindingContext;

    private async void ButtonPlaylist_Clicked(object sender, EventArgs e)
    {
        ViewModel.IsMenuOpen = false;

        await Shell.Current.GoToAsync(nameof(PlaylistPage));
    }

    public bool Back()
    {
        if (ViewModel.IsMenuOpen)
        {
            ViewModel.ToogleMenu();
            return true;
        }

        return false;
    }
}
