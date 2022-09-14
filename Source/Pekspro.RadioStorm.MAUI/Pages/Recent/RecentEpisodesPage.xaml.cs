namespace Pekspro.RadioStorm.MAUI.Pages.Recent;

public partial class RecentEpisodesPage : ContentPage
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

    async private void RecentTapped(object sender, EventArgs e)
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

    protected override bool OnBackButtonPressed()
    {
        ((AppShell)Shell.Current).GoToFavorites();

        return true;
    }
}
