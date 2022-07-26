namespace Pekspro.RadioStorm.MAUI.Pages.Player;

public partial class PlaylistPage : ContentPage
{
    public PlaylistPage(PlaylistViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected PlaylistViewModel ViewModel => (PlaylistViewModel) BindingContext;

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
