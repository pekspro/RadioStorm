namespace Pekspro.RadioStorm.MAUI.Pages.Episode;

public partial class DownloadsPage : ContentPage
{
    public DownloadsPage(DownloadsViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }

    protected DownloadsViewModel ViewModel => (DownloadsViewModel) BindingContext;

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
