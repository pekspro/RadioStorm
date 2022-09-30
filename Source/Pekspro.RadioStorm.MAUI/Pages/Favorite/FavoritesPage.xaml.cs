namespace Pekspro.RadioStorm.MAUI.Pages.Favorite;

public sealed partial class FavoritesPage : ContentPage
{
    public FavoritesPage(FavoritesViewModel viewModel, SynchronizingViewModel synchronizingViewModel)
    { 
        InitializeComponent();

        BindingContext = viewModel;
        SynchronizingViewModel = synchronizingViewModel;

        ToolBarItemSynchronize.BindingContext = synchronizingViewModel;
        ProgressSynchronize.BindingContext = synchronizingViewModel;
    }

    private FavoritesViewModel ViewModel => (FavoritesViewModel) BindingContext;

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

    private async void ButtonWelcomeModeChannels_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ChannelsPage));
    }

    private async void ButtonWelcomeModePrograms_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProgramsPage));
    }

    private async void ButtonWelcomeModeSettings_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        SwipeHelper.SwipeStarted(sender);
    }

    private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        SwipeHelper.SwipeEnded(sender);
    }
}
