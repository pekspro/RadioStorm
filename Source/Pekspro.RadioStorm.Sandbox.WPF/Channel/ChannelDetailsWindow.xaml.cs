namespace Pekspro.RadioStorm.Sandbox.WPF;

public sealed partial class ChannelDetailsWindow : Window
{
    public ChannelDetailsWindow(ChannelDetailsViewModel channelInfoViewModel,
            SongsViewModel songsViewModel,
            SchedulesEpisodesViewModel schedulesEpisodesViewModel
        )
    {
        InitializeComponent();

        StartParameter = ChannelDetailsViewModel.CreateStartParameter(132, null, null, null);

        TabSongList.DataContext = songsViewModel;
        TabScheduledEpisodes.DataContext = schedulesEpisodesViewModel;

        DataContext = channelInfoViewModel;
    }

    private ChannelDetailsViewModel ViewModel => (ChannelDetailsViewModel) DataContext;
    private SongsViewModel SongsViewModel => (SongsViewModel) TabSongList.DataContext;
    private SchedulesEpisodesViewModel SchedulesEpisodesViewModel => (SchedulesEpisodesViewModel) TabScheduledEpisodes.DataContext;

    public string StartParameter { get; set; }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        ViewModel.OnNavigatedTo(StartParameter);
        SongsViewModel.OnNavigatedTo(true, ViewModel.ChannelId);
        SchedulesEpisodesViewModel.OnNavigatedTo(StartParameter);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        ViewModel.OnNavigatedFrom();
        SongsViewModel.OnNavigatedFrom();
        SchedulesEpisodesViewModel.OnNavigatedFrom();
    }
}
