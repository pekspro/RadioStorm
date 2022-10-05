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

#if ANDROID
        // TODO: Remove when fixed: https://github.com/dotnet/maui/issues/10452
    
        ToolbarItemVerySlow.IsVisible = false;
        ToolbarItemSlow.IsVisible = false;
        ToolbarItemNormal.IsVisible = false;
        ToolbarItemFast.IsVisible = false;
        ToolbarItemVeryFast.IsVisible = false;

        Application.Current!.Dispatcher.Dispatch(() =>
        {
            ToolbarItemVerySlow.IsVisible = true;
            ToolbarItemSlow.IsVisible = true;
            ToolbarItemNormal.IsVisible = true;
            ToolbarItemFast.IsVisible = true;
            ToolbarItemVeryFast.IsVisible = true;
        });
#endif 
        
    }
}
