namespace Pekspro.RadioStorm.MAUI.Pages.Recent;

public sealed partial class RecentEpisodesPage : ContentPage
{
    public RecentEpisodesPage(RecentEpisodesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private RecentEpisodesViewModel ViewModel => (RecentEpisodesViewModel) BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo();
    }

    private void MenuItemRemoveFromRecentTapped(object sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is RecentEpisodeModel recent)
        {
            ViewModel.RemoveFromRecentList(recent.Model);
        }
    }

    async private void EpisodeTapped(object sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is EpisodeModel episode)
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

    protected override bool OnBackButtonPressed()
    {
        ((AppShell)Shell.Current).GoToFavorites();

        return true;
    }
}
