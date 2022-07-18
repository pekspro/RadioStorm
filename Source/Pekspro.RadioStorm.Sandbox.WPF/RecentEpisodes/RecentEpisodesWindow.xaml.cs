using Pekspro.RadioStorm.Sandbox.WPF.Episode;

namespace Pekspro.RadioStorm.Sandbox.WPF.RecentEpisodes
{
    /// <summary>
    /// Interaction logic for RecentEpisodesWindow.xaml
    /// </summary>
    public partial class RecentEpisodesWindow : Window
    {
        public RecentEpisodesWindow(RecentEpisodesViewModel programInfoViewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            DataContext = programInfoViewModel;
            ServiceProvider = serviceProvider;
        }

        protected RecentEpisodesViewModel ViewModel => (RecentEpisodesViewModel)DataContext;

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

        private void ListViewEpisodes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectionModeHelper.SelectionCount = ListViewEpisodes.SelectedItems.Count;
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

        private void MenuItemMultipleAddToPlayList_Click(object sender, RoutedEventArgs e)
        {
            foreach (EpisodeModel model in ListViewEpisodes.SelectedItems)
            {
                model.AddToPlayList();
            }
        }

        private void MenuItemMultipleRemoveFromRecentList_Click(object sender, RoutedEventArgs e)
        {
            List<EpisodeModel> models = new List<EpisodeModel>();
            foreach (EpisodeModel model in ListViewEpisodes.SelectedItems)
            {
                models.Add(model);
            }
                
            ViewModel.RemoveFromRecentList(models.ToArray());
        }
    }
}
