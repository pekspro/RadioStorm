namespace Pekspro.RadioStorm.Sandbox.WPF;

public partial class ChannelsWindow : Window
{
    public ChannelsWindow(ChannelsViewModel channelInfoViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = channelInfoViewModel;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected ChannelsViewModel ViewModel => (ChannelsViewModel) DataContext;

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

    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var c = ListViewChannels.SelectedItem as ChannelModel;

        if (c is not null)
        {
            var channelWindow = ServiceProvider.GetRequiredService<ChannelDetailsWindow>();
            channelWindow.StartParameter = ChannelDetailsViewModel.CreateStartParameter(c);
            channelWindow.Show();
        }
    }

    private void MenuItemMultipleSetAsFavorite_Click(object sender, RoutedEventArgs e)
    {
        foreach (ChannelModel model in ListViewChannels.SelectedItems)
        {
            model.IsFavorite = true;
        }
    }

    private void MenuItemMultipleRemoveAsFavorite_Click(object sender, RoutedEventArgs e)
    {
        foreach (ChannelModel model in ListViewChannels.SelectedItems)
        {
            model.IsFavorite = false;
        }
    }

    private void ListViewChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectionModeHelper.SelectionCount = ListViewChannels.SelectedItems.Count;
    }
}
