namespace Pekspro.RadioStorm.MAUI.Pages.Player;

public sealed partial class CurrentPlayingPage : ContentPage
{
    public CurrentPlayingPage(CurrentPlayingViewModel currentPlayingViewModel)
    {
        InitializeComponent();

        BindingContext = currentPlayingViewModel;
    }

    private CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel )BindingContext;

    private async void ButtonPlaylist_Clicked(object sender, EventArgs e)
    {
        // Pop current page on the stack and add the new page
        var page = Navigation.NavigationStack.LastOrDefault();

        await Shell.Current.GoToAsync(nameof(PlaylistPage));

        Navigation.RemovePage(page);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        this.FixToolbarItems();
    }

    private void ToolbarStop_Clicked(object sender, EventArgs e)
    {
        ViewModel.PlayerViewModel.Stop();

        ((AppShell)Shell.Current).GoToFavorites();
    }
}
