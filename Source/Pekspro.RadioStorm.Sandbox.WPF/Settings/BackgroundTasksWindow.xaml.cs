using Pekspro.RadioStorm.CacheDatabase;

namespace Pekspro.RadioStorm.Sandbox.WPF.Settings;

/// <summary>
/// Interaction logic for BackgroundTasksWindow.xaml
/// </summary>
public partial class BackgroundTasksWindow : Window
{
    public BackgroundTasksWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    private void ButtonStartPrefetch_Click(object sender, RoutedEventArgs e)
    {
        var prefetcher = ServiceProvider.GetRequiredService<ICachePrefetcher>();

        _ = prefetcher.PrefetchAsync(default);
    }

    private void ButtonDeleteObseleteDownloads_Click(object sender, RoutedEventArgs e)
    {
        var autoDeleteManager = ServiceProvider.GetRequiredService<IAutoDownloadDeleteManager>();

        autoDeleteManager.DeleteObseleteDownloads();
    }
}
