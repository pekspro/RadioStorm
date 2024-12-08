namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ScheduledEpisodesPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

    public ScheduledEpisodesPage(SchedulesEpisodesViewModel viewModel, IMessenger messenger, IMainThreadRunner mainThreadRunner)
    {
        InitializeComponent();
        
        BindingContext = viewModel;

        messenger.Register<SemiCompletedScheduleEpisodesListLoaded>(this, (a, b) =>
        {
            mainThreadRunner.RunInMainThread(() =>
            {
                if (ViewModel.ChannelId == b.ChannelId)
                {
                    // TODO: Not working properly on Android. See this issue: https://github.com/dotnet/maui/issues/8718
                    ListViewScheduledEpisodes.ScrollTo(b.IndexOfFirstIncompleted, -1, ScrollToPosition.Center, false);
                }
            });
        });

    }

    private SchedulesEpisodesViewModel ViewModel => (SchedulesEpisodesViewModel) BindingContext;

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
}
