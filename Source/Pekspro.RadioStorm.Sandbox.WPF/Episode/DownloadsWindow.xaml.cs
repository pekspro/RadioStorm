﻿namespace Pekspro.RadioStorm.Sandbox.WPF.Episode;

public sealed partial class DownloadsWindow : Window
{
    public DownloadsWindow(DownloadsViewModel programInfoViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = programInfoViewModel;
        ServiceProvider = serviceProvider;
    }

    private DownloadsViewModel ViewModel => (DownloadsViewModel)DataContext;

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
            programWindow.StartParameter = EpisodeDetailsViewModel.CreateStartParameter(p, true);
            programWindow.Show();
        }
    }

    private void ListViewEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectionModeHelper.SelectionCount = ListViewEpisodes.SelectedItems.Count;
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
        foreach (EpisodeModel model in GetSelectedEpisodes())
        {
            model.IsListened = true;
        }
    }

    private void MenuItemMultipleSetNotListened_Click(object sender, RoutedEventArgs e)
    {
        foreach (EpisodeModel model in GetSelectedEpisodes())
        {
            model.IsListened = false;
        }
    }

    private void MenuItemMultipleAddToPlayList_Click(object sender, RoutedEventArgs e)
    {
        foreach (EpisodeModel model in GetSelectedEpisodes())
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
}
