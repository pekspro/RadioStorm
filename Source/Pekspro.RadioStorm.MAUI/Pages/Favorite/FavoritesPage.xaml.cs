namespace Pekspro.RadioStorm.MAUI.Pages.Favorite;

public sealed partial class FavoritesPage : ContentPage
{
    public FavoritesPage(FavoritesViewModel viewModel, SynchronizingViewModel synchronizingViewModel, ILocalSettings localSettings)
    { 
        InitializeComponent();

        BindingContext = viewModel;
        SynchronizingViewModel = synchronizingViewModel;
        ToolBarItemSynchronize.BindingContext = synchronizingViewModel;
        ProgressSynchronize.BindingContext = synchronizingViewModel;

        LocalSettings = localSettings;
        _AlbumMode = LocalSettings.FavoriteAlbumMode;
        UpdateListMode();

        SizeChanged += (a, b) => UpdateListMode();
    }

    private FavoritesViewModel ViewModel => (FavoritesViewModel) BindingContext;

    public SynchronizingViewModel SynchronizingViewModel { get; }
    
    public ILocalSettings LocalSettings { get; }

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
        var channel = (sender as ChannelControl)?.BindingContext as ChannelModel ??
                      (sender as ChannelAlbumControl)?.BindingContext as ChannelModel;
        
        if (channel is not null)
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
        var program = (sender as ProgramControl)?.BindingContext as ProgramModel ??
                      (sender as FavoriteProgramAlbumControl)?.BindingContext as ProgramModel;
        
        if (program is not null)
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

    private void ToolbarItemListMode_Clicked(object sender, EventArgs e)
    {
        AlbumMode = false;
    }

    private void ToolbarItemAlbumMode_Clicked(object sender, EventArgs e)
    {
        AlbumMode = true;
    }

    private bool _AlbumMode;

    private bool AlbumMode
    {
        get => _AlbumMode;        
        set
        {
            if (_AlbumMode != value)
            {
                _AlbumMode = value;
                LocalSettings.FavoriteAlbumMode = value;

                UpdateListMode();
            }
        }
    }

    int _CurrentMode = -1;

    private void UpdateListMode()
    {
        int expectedMode = 0;

        if (AlbumMode)
        {
            expectedMode = Width switch 
            {
                >= 180 * 6 => 6,
                >= 180 * 5 => 5,
                >= 180 * 4 => 4,
                >= 180 * 3 => 3,
                >= 180 * 2 => 2,
                _ => 0
            };
        }

        if (expectedMode != _CurrentMode)
        {
            _CurrentMode = expectedMode;

            RefreshViewList.IsVisible = expectedMode == 0;
            RefreshViewAlbum2.IsVisible = expectedMode == 2;
            RefreshViewAlbum3.IsVisible = expectedMode == 3;
            RefreshViewAlbum4.IsVisible = expectedMode == 4;
            RefreshViewAlbum5.IsVisible = expectedMode == 5;
            RefreshViewAlbum6.IsVisible = expectedMode == 6;
        }
    }
}
