
namespace Pekspro.RadioStorm.MAUI.Pages.Favorite;

public sealed partial class FavoritesPage : ContentPage
{
    public FavoritesPage(FavoritesViewModel viewModel, ReviewViewModel reviewViewModel, SynchronizingViewModel synchronizingViewModel, ILocalSettings localSettings)
    {
        InitializeComponent();

        BindingContext = viewModel;

        SynchronizingViewModel = synchronizingViewModel;
        ProgressSynchronize.BindingContext = synchronizingViewModel;
        ToolbarHelperHasAnyRemoteSignedInProvider.BindingContext =
        ToolbarItemSynchronize.BindingContext = synchronizingViewModel;
        
        ReviewViewModel = reviewViewModel;
        ReviewBorder.BindingContext = reviewViewModel;

        LocalSettings = localSettings;
        _AlbumMode = LocalSettings.FavoriteAlbumMode;
        UpdateListMode();

        SizeChanged += (a, b) => UpdateListMode();
    }

    private FavoritesViewModel ViewModel => (FavoritesViewModel)BindingContext;

    public SynchronizingViewModel SynchronizingViewModel { get; }
    
    public ReviewViewModel ReviewViewModel { get; }

    public ILocalSettings LocalSettings { get; }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        ViewModel.OnNavigatedTo();
        
        RefreshToolbarItems();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }

    private void RefreshToolbarItems()
    {
        this.SetToolbarItemVisibility(ToolbarItemListMode, ViewModel.HasFavorites && ViewModel.HasAlbumViewSupport);
        this.SetToolbarItemVisibility(ToolbarItemAlbumMode, ViewModel.HasFavorites && ViewModel.HasAlbumViewSupport);
        this.SetToolbarItemVisibility(ToolbarItemSynchronize, SynchronizingViewModel?.HasAnyRemoteSignedInProvider == true);
        this.SetToolbarItemVisibility(ToolbarItemUpdate, ViewModel.HasFavorites);
    }

    private void ToolbarHelper_ToggleChanged(object sender, ToggledEventArgs e)
    {
        RefreshToolbarItems();
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
        var program = (sender as FavoriteProgramControl)?.BindingContext as ProgramModel ??
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

    int _CurrenColumnCount = -1;

    private void UpdateListMode()
    {
        int expectedColumnCount = 0;
        const int itemMargin = 4;
        // const int normalMinItemWidth = 180;
        const int normalMinItemWidth = 168;
        const int thinMinItemWidth = 160;

#if WINDOWS
        // To much margin on Windows:
        // TODO: Remove this when fixed: https://github.com/dotnet/maui/issues/11320
        const int sideMargin = 8 + 8 + 32;
#else
        const int sideMargin = 16 + 16;
#endif

        if (AlbumMode)
        {
            var usableSize = Width - sideMargin;

            expectedColumnCount = usableSize switch
            {
                //>= normalMinItemWidth * 8 + itemMargin * 7 => 8,
                //>= normalMinItemWidth * 7 + itemMargin * 6 => 7,
                //>= normalMinItemWidth * 6 + itemMargin * 5 => 6,
                //>= normalMinItemWidth * 5 + itemMargin * 4 => 5,
                //>= normalMinItemWidth * 4 + itemMargin * 3 => 4,
                >= normalMinItemWidth * 4 + itemMargin * 3 => (int) ((usableSize + itemMargin) / (normalMinItemWidth + itemMargin)),
                >= thinMinItemWidth   * 3 + itemMargin * 2 => 3,
                >= thinMinItemWidth   * 2 + itemMargin * 1 => 2,
                _ => 0
            };
        }

        ViewModel.HasAlbumViewSupport = (Width - sideMargin) >= thinMinItemWidth * 2 + itemMargin * 1;

        if (expectedColumnCount != _CurrenColumnCount)
        {
            _CurrenColumnCount = expectedColumnCount;

            RefreshViewList.IsVisible = expectedColumnCount == 0;

            RefreshViewAlbum.IsVisible = expectedColumnCount >= 2;

            AlbumGridLayout.Span = expectedColumnCount;
        }

        if (expectedColumnCount > 0)
        {
            double realItemWidth = (Width - sideMargin - itemMargin * (expectedColumnCount-1)) / expectedColumnCount;

            ViewModel.AlbumCardWidth = (int) realItemWidth;
            // Original height - original image + new width - horisontal margin.
            ViewModel.AlbumCardHeight = (int) Math.Min(232 - 140 + realItemWidth - 16, 260);
        }
    }
}
