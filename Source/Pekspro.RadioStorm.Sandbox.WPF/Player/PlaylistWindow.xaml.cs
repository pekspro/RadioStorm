using Pekspro.RadioStorm.Sandbox.WPF.Episode;

namespace Pekspro.RadioStorm.Sandbox.WPF.Player;

public partial class PlaylistWindow : Window
{
    public PlaylistWindow(PlaylistViewModel playlistViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = playlistViewModel;
        ServiceProvider = serviceProvider;
    }

    protected PlaylistViewModel ViewModel => (PlaylistViewModel)DataContext;

    public IServiceProvider ServiceProvider { get; }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        ViewModel.OnNavigatedTo();
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
            var programWindow = ServiceProvider.GetRequiredService<EpisodeDetailsWindow>();
            programWindow.StartParameter = EpisodeDetailsViewModel.CreateStartParameter(p, false);
            programWindow.Show();
        }
    }

    private void MenuItemMultipleSetListened_Click(object sender, RoutedEventArgs e)
    {
        foreach (EpisodeModel model in ListViewEpisodes.SelectedItems)
        {
            model.IsListened = true;
        }
    }

    private void MenuItemMultipleSetNotListened_Click(object sender, RoutedEventArgs e)
    {
        foreach (EpisodeModel model in ListViewEpisodes.SelectedItems)
        {
            model.IsListened = false;
        }
    }

    private void MenuItemMultipleRemoveFromRecentList_Click(object sender, RoutedEventArgs e)
    {
        List<EpisodeModel> models = new List<EpisodeModel>();
        foreach (EpisodeModel model in ListViewEpisodes.SelectedItems)
        {
            models.Add(model);
        }

        ViewModel.RemoveFromPlaylist(models.ToArray());
    }
}

