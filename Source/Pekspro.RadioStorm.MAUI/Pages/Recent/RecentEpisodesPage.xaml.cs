namespace Pekspro.RadioStorm.MAUI.Pages.Recent;

public partial class RecentEpisodesPage : ContentPage
{
    public RecentEpisodesPage(RecentEpisodesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected RecentEpisodesViewModel ViewModel => BindingContext as RecentEpisodesViewModel;

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
}
