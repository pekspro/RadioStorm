namespace Pekspro.RadioStorm.Sandbox.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    private void ButtonFavorites_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<FavoritesWindow>();
        win.Show();
    }

    private void ButtonChannels_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<ChannelsWindow>();
        win.Show();
    }

    private void ButtonPrograms_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<ProgramsWindow>();
        win.Show();
    }

    private void ButtonDownloads_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<DownloadsWindow>();
        win.Show();
    }

    private void ButtonRecentEpisodes_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<RecentEpisodesWindow>();
        win.Show();
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<SettingsWindow>();
        win.Show();
    }

    private void ButtonLogs_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<LoggingWindow>();
        win.Show();
    }

    private void ButtonBackgroundTasks_Click(object sender, RoutedEventArgs e)
    {
        var win = ServiceProvider.GetRequiredService<BackgroundTasksWindow>();
        win.Show();
    }
}
