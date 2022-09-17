using Pekspro.RadioStorm.Sandbox.WPF.Episode;

namespace Pekspro.RadioStorm.Sandbox.WPF.Program;

public sealed partial class ProgramDetailsWindow : Window
{
    public ProgramDetailsWindow(ProgramDetailsViewModel programDetailsViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        StartParameter = ProgramDetailsViewModel.CreateStartParameter(132, "XYZ");

        DataContext = programDetailsViewModel;
        ServiceProvider = serviceProvider;
    }

    private ProgramDetailsViewModel ViewModel => (ProgramDetailsViewModel)DataContext;

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

    private void ListViewEpisodes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var p = ListViewEpisodes.SelectedItem as EpisodeModel;

        if (p is not null)
        {
            var episodeWindow = ServiceProvider.GetRequiredService<EpisodeDetailsWindow>();
            episodeWindow.StartParameter = EpisodeDetailsViewModel.CreateStartParameter(p, false);
            episodeWindow.Show();
        }
    }

    private void ListViewEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.EpisodesViewModel.SelectionModeHelper.SelectionCount = ListViewEpisodes.SelectedItems.Count;
    }

    private List<EpisodeModel> GetSelectedEpisodes()
    {
        var episodes = new List<EpisodeModel>();

        foreach (EpisodeModel model in ListViewEpisodes.SelectedItems)
        {
            episodes.Add(model);
        }

        return episodes;
    }
    
    private void MenuItemMultipleSetListened_Click(object sender, RoutedEventArgs e)
    {
        foreach(EpisodeModel model in GetSelectedEpisodes())
        {
            model.IsListened = true;
        }
    }

    private void MenuItemMultipleSetNotListened_Click(object sender, RoutedEventArgs e)
    {
        foreach(EpisodeModel model in GetSelectedEpisodes())
        {
            model.IsListened = false;
        }
    }

    private void MenuItemMultipleAddToPlayList_Click(object sender, RoutedEventArgs e)
    {
        foreach(EpisodeModel model in GetSelectedEpisodes())
        {
            model.AddToPlayList();
        }
    }

    private void MenuItemMultipleDownload_Click(object sender, RoutedEventArgs e)
    {
        foreach (EpisodeModel model in GetSelectedEpisodes())
        {
            model.Download();
        }
    }

    private void MenuItemMultipleDeleteDownload_Click(object sender, RoutedEventArgs e)
    {
        foreach (EpisodeModel model in GetSelectedEpisodes())
        {
            model.DeleteDownload();
        }
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e)
    {
        var programWindow = ServiceProvider.GetRequiredService<ProgramSettingsWindow>();
        programWindow.StartParameter = ProgramSettingsViewModel.CreateStartParameter(ViewModel.ProgramData);
        programWindow.Show();
    }
}
