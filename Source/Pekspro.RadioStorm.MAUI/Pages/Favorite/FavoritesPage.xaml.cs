namespace Pekspro.RadioStorm.MAUI.Pages.Favorite;

public partial class FavoritesPage : ContentPage
{
    public FavoritesPage(FavoritesViewModel viewModel, SynchronizingViewModel synchronizingViewModel)
    { 
        InitializeComponent();

        BindingContext = viewModel;
        SynchronizingViewModel = synchronizingViewModel;

        ToolBarItemSynchronize.BindingContext = synchronizingViewModel;
        ProgressSynchronize.BindingContext = synchronizingViewModel;
    }

    protected FavoritesViewModel ViewModel => BindingContext as FavoritesViewModel;

    public SynchronizingViewModel SynchronizingViewModel { get; }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }

    async void ChannelTapped(object sender, EventArgs e)
    {
        if ((sender as ChannelControl)?.BindingContext is ChannelModel channel)
        {
            string param = ChannelDetailsViewModel.CreateStartParameter(channel);

            await Shell.Current.GoToAsync(nameof(ChannelDetailsPage), new Dictionary<string, object>()
            {
                { "Data", param }
            });
        }
    }

    async private void ProgramTapped(object sender, EventArgs e)
    {
        if ((sender as FavoriteProgramControl)?.BindingContext is ProgramModel program)
        {
            string param = ProgramDetailsViewModel.CreateStartParameter(program);

            await Shell.Current.GoToAsync(nameof(ProgramDetailsPage), new Dictionary<string, object>()
            {
                { "Data", param }
            });
        }
    }

    private async void ButtonWelcomeModeChannels_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ChannelsPage));
    }

    private async void ButtonWelcomeModePrograms_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProgramsPage));
    }

    private async void ButtonWelcomeModeSettings_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    private void ButtonWelcomeModeFeedback_Clicked(object sender, EventArgs e)
    {

    }
}
