namespace Pekspro.RadioStorm.MAUI.Views;

public partial class PlayerControl
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

    protected PlayerViewModel ViewModel => (PlayerViewModel) BindingContext;

    private async void ButtonPlaylist_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PlaylistPage));
    }
}
