namespace Pekspro.RadioStorm.Sandbox.WPF.Episode;

public partial class EpisodeDetailsWindow : Window
{
    public EpisodeDetailsWindow(EpisodeDetailsViewModel episodeDetailsViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        StartParameter = EpisodeDetailsViewModel.CreateStartParameter(49265, "XYZ", true);

        DataContext = episodeDetailsViewModel;
        ServiceProvider = serviceProvider;
    }

    private EpisodeDetailsViewModel ViewModel => (EpisodeDetailsViewModel)DataContext;

    public string StartParameter { get; set; }
    public IServiceProvider ServiceProvider { get; }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        ViewModel.OnNavigatedTo(StartParameter);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        ViewModel.OnNavigatedFrom();
    }

    private void ProgramHyperlink_Click(object sender, RoutedEventArgs e)
    {
        var programWindow = ServiceProvider.GetRequiredService<ProgramDetailsWindow>();
        programWindow.StartParameter = ProgramDetailsViewModel.CreateStartParameter(ViewModel.EpisodeData.ProgramId ?? 0, ViewModel.EpisodeData.ProgramName);
        programWindow.Show();
    }

    private void PreviousEpisodeHyperlink_Click(object sender, RoutedEventArgs e)
    {
        LoadEpisode(ViewModel.PreviousEpisodeData.Id);
    }

    private void NextEpisodeHyperlink_Click(object sender, RoutedEventArgs e)
    {
        LoadEpisode(ViewModel.NextEpisodeData.Id);
    }

    private void LoadEpisode(int id)
    {
        StartParameter = EpisodeDetailsViewModel.CreateStartParameter(id, String.Empty, ViewModel.NavigationToProgramPageIsPossible);
        DataContext = ServiceProvider.GetRequiredService<EpisodeDetailsViewModel>();
        ViewModel.OnNavigatedTo(StartParameter);
    }
}
