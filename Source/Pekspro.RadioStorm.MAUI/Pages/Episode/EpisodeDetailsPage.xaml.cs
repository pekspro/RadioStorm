namespace Pekspro.RadioStorm.MAUI.Pages.Episode;

[QueryProperty(nameof(Data), nameof(Data))]
public sealed partial class EpisodeDetailsPage : ContentPage
{
    public string Data { get; set; } = null!;

    public EpisodeDetailsPage(EpisodeDetailsViewModel viewModel)
    {
        InitializeComponent();
        
        WidthStateHelper.ConfigureWidthState(GridLayout, this);
        
        BindingContext = viewModel;
    }

    private EpisodeDetailsViewModel ViewModel => (EpisodeDetailsViewModel) BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo(Data);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }

    private async void ButtonShowPlayList_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PlaylistPage));
    }

    private async void ButtonShowNextEpisode_Click(object sender, EventArgs args)
    {
        if (ViewModel.NextEpisodeData is not null)
        {
            string param = EpisodeDetailsViewModel.CreateStartParameter(ViewModel.NextEpisodeData, false);

            // Pop current page on the stack and add the new page
            var page = Navigation.NavigationStack.LastOrDefault();

            await Shell.Current.GoToAsync(nameof(EpisodeDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });
            
            Navigation.RemovePage(page);
        }
    }

    private async void ButtonShowPreviousEpisode_Click(object sender, EventArgs args)
    {
        if (ViewModel.PreviousEpisodeData is not null)
        {
            string param = EpisodeDetailsViewModel.CreateStartParameter(ViewModel.PreviousEpisodeData, false);

            // Pop current page on the stack and add the new page
            var page = Navigation.NavigationStack.LastOrDefault();
            
            await Shell.Current.GoToAsync(nameof(EpisodeDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });

            Navigation.RemovePage(page);
        }
    }

    private async void ButtonShowMoreEpisodes_Click(object sender, EventArgs args)
    {
        if (ViewModel.EpisodeData?.ProgramId is not null)
        {
            string param = ProgramDetailsViewModel.CreateStartParameter(ViewModel.EpisodeData.ProgramId.Value, ViewModel.EpisodeData.ProgramName);

            await Shell.Current.GoToAsync(nameof(ProgramDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });
        }
    }
}
