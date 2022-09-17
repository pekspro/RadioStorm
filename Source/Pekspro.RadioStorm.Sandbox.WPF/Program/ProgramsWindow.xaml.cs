namespace Pekspro.RadioStorm.Sandbox.WPF.Program;

public sealed partial class ProgramsWindow : Window
{
    public ProgramsWindow(ProgramsViewModel programInfoViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = programInfoViewModel;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    private ProgramsViewModel ViewModel => (ProgramsViewModel)DataContext;

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
        var p = ListViewPrograms.SelectedItem as ProgramModel;

        if (p is not null)
        {
            var programWindow = ServiceProvider.GetRequiredService<ProgramDetailsWindow>();
            programWindow.StartParameter = ProgramDetailsViewModel.CreateStartParameter(p);
            programWindow.Show();
        }
    }

    private void MenuItemMultipleSetAsFavorite_Click(object sender, RoutedEventArgs e)
    {
        foreach (ProgramModel model in ListViewPrograms.SelectedItems)
        {
            model.IsFavorite = true;
        }
    }

    private void MenuItemMultipleRemoveAsFavorite_Click(object sender, RoutedEventArgs e)
    {
        foreach (ProgramModel model in ListViewPrograms.SelectedItems)
        {
            model.IsFavorite = false;
        }
    }

    private void ListViewPrograms_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SelectionModeHelper.SelectionCount = ListViewPrograms.SelectedItems.Count;
    }
}
