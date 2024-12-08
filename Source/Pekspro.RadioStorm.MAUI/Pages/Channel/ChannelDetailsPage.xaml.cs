
namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ChannelDetailsPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

    public ChannelDetailsPage(ChannelDetailsViewModel viewModel)
    {
        InitializeComponent();

        WidthStateHelper.ConfigureWidthState(GridLayout, this);
        
        BindingContext = viewModel;
    }

    private ChannelDetailsViewModel ViewModel => (ChannelDetailsViewModel) BindingContext;

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

    private async void ButtonOpenCurrentProgram_Click(object sender, EventArgs e)
    {
        if (ViewModel?.ChannelData?.Status?.CurrentProgramId is not null)
        {
            string startParameter = ProgramDetailsViewModel.CreateStartParameter(ViewModel.ChannelData.Status.CurrentProgramId.Value, ViewModel?.ChannelData?.Status?.CurrentProgram);

            if (startParameter is not null)
            {
                await Shell.Current.GoToAsync(nameof(ProgramDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", startParameter }
                });
            }
        }
    }

    private async void ButtonOpenNextProgram_Click(object sender, EventArgs e)
    {
        if (ViewModel?.ChannelData?.Status?.NextProgramId is not null)
        {
            string startParameter = ProgramDetailsViewModel.CreateStartParameter(ViewModel.ChannelData.Status.NextProgramId.Value, ViewModel?.ChannelData?.Status?.NextProgram);

            if (startParameter is not null)
            {
                await Shell.Current.GoToAsync(nameof(ProgramDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", startParameter }
                });
            }
        }
    }

    private async void ToolbarItemOpenScheduleEpisodes_Click(object sender, EventArgs e)
    {
        if (ViewModel?.ChannelData is not null)
        {
            string startParameter = ChannelDetailsViewModel.CreateStartParameter(ViewModel.ChannelData);

            if (startParameter is not null)
            {
                await Shell.Current.GoToAsync(nameof(ScheduledEpisodesPage), new Dictionary<string, object>()
                {
                    { "Data", startParameter }
                });
            }
        }
    }

    private async void ToolbarItemOpenSongList_Click(object sender, EventArgs e)
    {
        if (ViewModel?.ChannelData is not null)
        {
            string startParameter = ChannelDetailsViewModel.CreateStartParameter(ViewModel.ChannelData);

            if (startParameter is not null)
            {
                await Shell.Current.GoToAsync(nameof(ChannelSongListPage), new Dictionary<string, object>()
                {
                    { "Data", startParameter }
                });
            }
        }
    }
}
