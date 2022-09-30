namespace Pekspro.RadioStorm.MAUI.Pages.Player;

public sealed partial class CurrentPlayingPage : ContentPage
{
    public CurrentPlayingPage(PlayerViewModel playerViewModel)
    {
        InitializeComponent();

        BindingContext = playerViewModel;
    }

    private PlayerViewModel ViewModel => (PlayerViewModel)BindingContext;

    private async void ButtonPlaylist_Clicked(object sender, EventArgs e)
    {
        // Pop current page on the stack and add the new page
        var page = Navigation.NavigationStack.LastOrDefault();

        await Shell.Current.GoToAsync(nameof(PlaylistPage));

        Navigation.RemovePage(page);
    }
}
