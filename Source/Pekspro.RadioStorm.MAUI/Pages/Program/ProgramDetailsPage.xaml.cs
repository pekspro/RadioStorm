namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public sealed partial class ProgramDetailsPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

    public ProgramDetailsPage(ProgramDetailsViewModel viewModel)
    {
        InitializeComponent();
        
        WidthStateHelper.ConfigureWidthState(GridLayout, this);

        BindingContext = viewModel;
    }

    private ProgramDetailsViewModel ViewModel => (ProgramDetailsViewModel) BindingContext;

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

    async private void EpisodeTapped(object sender, EventArgs e)
    {
        if ((sender as EpisodeControl)?.BindingContext is EpisodeModel episode)
        {
            string param = EpisodeDetailsViewModel.CreateStartParameter(episode, false);

            await Shell.Current.GoToAsync(nameof(EpisodeDetailsPage), new Dictionary<string, object>()
            {
                { "Data", param }
            });
        }
    }
    
    async private void ToolbarItemSettings_Clicked(object sender, EventArgs e)
    {
        if (ViewModel.ProgramData is null)
        {
            return;
        }

        string param = ProgramSettingsViewModel.CreateStartParameter(ViewModel.ProgramData);

        await Shell.Current.GoToAsync(nameof(ProgramSettingsPage), new Dictionary<string, object>()
        {
            { "Data", param }
        });
    }

    private void ButtonScrollToFirstNotListened_Clicked(object sender, EventArgs e)
    {
        int? position = ViewModel.EpisodesViewModel.FirstNotListenedEpisodePosition;

        if (position is not null)
        {
#if ANDROID
            // TODO: Remote below workaround when this issue is fixed: https://github.com/dotnet/maui/issues/8718
            var recyclerView = EpisodesListView.Handler!.PlatformView as AndroidX.RecyclerView.Widget.RecyclerView;

            int extra = 0;
            int prevGroupSum = 0;

            for (int i = 0; i < ViewModel.EpisodesViewModel.GroupedItems!.Count; i++)
            {
                var group = ViewModel.EpisodesViewModel.GroupedItems[i];

                if (position.Value < prevGroupSum + group.Count)
                {
                    extra = i;
                    break;
                }

                prevGroupSum += group.Count;
            }

            // recyclerView.ScrollToPosition(position.Value + extra + 1);

            var layoutManager = recyclerView!.GetLayoutManager() as AndroidX.RecyclerView.Widget.LinearLayoutManager;
            layoutManager!.ScrollToPositionWithOffset(position.Value + extra + 1, 0);
            
#else
            EpisodesListView.ScrollTo(position.Value, position: ScrollToPosition.Center);
#endif
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
}
