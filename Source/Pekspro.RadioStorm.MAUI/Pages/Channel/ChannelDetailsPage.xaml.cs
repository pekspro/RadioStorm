namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

[QueryProperty(nameof(Data), nameof(Data))]
public sealed partial class ChannelDetailsPage : ContentPage
{
    public string Data { get; set; } = null!;

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

    private async void ButtonOpenScheduleEpisodes_Click(object sender, EventArgs e)
    {
        if (ViewModel?.ChannelId is not null)
        {
            await Shell.Current.GoToAsync(nameof(ScheduledEpisodesPage), new Dictionary<string, object>()
            {
                { "Data", ViewModel.ChannelId }
            });
        }
    }
}
