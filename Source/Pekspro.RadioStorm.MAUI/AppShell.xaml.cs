namespace Pekspro.RadioStorm.MAUI;

public sealed partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        BindingContext = this;
    }

    private string? selectedRoute;
    public string? SelectedRoute
    {
        get { return selectedRoute; }
        set
        {
            selectedRoute = value;
            OnPropertyChanged();
        }
    }

    async void OnMenuItemChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!string.IsNullOrEmpty(selectedRoute))
        {
            await Current.GoToAsync($"//{selectedRoute}");
        }
    }

    public void GoToFavorites()
    {
        SelectedRoute = "favorites";
        _ = Current.GoToAsync($"//favorites");
    }
}
