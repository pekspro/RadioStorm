using Pekspro.RadioStorm.UI.Model.Favorite;
using Pekspro.RadioStorm.UI.ViewModel.Favorite;

namespace Pekspro.RadioStorm.Sandbox.WPF.Favorite;

/// <summary>
/// Interaction logic for FavoritesWindow.xaml
/// </summary>
public sealed partial class FavoritesWindow : Window
{
    public FavoritesWindow(FavoritesViewModel favoritesViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = favoritesViewModel;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    private FavoritesViewModel ViewModel => (FavoritesViewModel) DataContext;

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
        if (ListViewFavorites.SelectedItem is ChannelModel c)
        {
            var channelWindow = ServiceProvider.GetRequiredService<ChannelDetailsWindow>();
            channelWindow.StartParameter = ChannelDetailsViewModel.CreateStartParameter(c);
            channelWindow.Show();
        }
        else if (ListViewFavorites.SelectedItem is ProgramModel p)
        {
            var channelWindow = ServiceProvider.GetRequiredService<ProgramDetailsWindow>();
            channelWindow.StartParameter = ProgramDetailsViewModel.CreateStartParameter(p);
            channelWindow.Show();
        }
    }

    private void MenuItemMultipleRemoveAsFavorite_Click(object sender, RoutedEventArgs e)
    {
        foreach (FavoriteBaseModel model in ListViewFavorites.SelectedItems)
        {
            model.IsFavorite = false;
        }
    }
}
