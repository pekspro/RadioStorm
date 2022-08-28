namespace Pekspro.RadioStorm.MAUI.Pages.Player;

public partial class PlaylistPage : ContentPage
{
    public PlaylistPage(PlaylistViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected PlaylistViewModel ViewModel => (PlaylistViewModel)BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo();
    }

    async private void EpisodeTapped(object sender, EventArgs e)
    {
        if ((sender as EpisodeControl)?.BindingContext is EpisodeModel episode)
        {
            string param = EpisodeDetailsViewModel.CreateStartParameter(episode, true);

            await Shell.Current.GoToAsync(nameof(EpisodeDetailsPage), new Dictionary<string, object>()
            {
                { "Data", param }
            });
        }
    }

    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        SwipeHelper.SwipeStarted(sender);
    }

    private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        SwipeHelper.SwipeEnded(sender);
    }

    private void SwipeItemRemoveFromPlayList_Clicked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeView)
        {
            if (swipeView.BindingContext is EpisodeModel episodeModel)
            {
                ViewModel.RemoveFromPlaylist(episodeModel);
            }
        }
    }
}

