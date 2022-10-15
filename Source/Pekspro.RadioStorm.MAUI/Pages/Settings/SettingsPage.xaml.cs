namespace Pekspro.RadioStorm.MAUI.Pages.Settings;

public sealed partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private SettingsViewModel ViewModel => (SettingsViewModel) BindingContext;

    protected override bool OnBackButtonPressed()
    {
        ((AppShell)Shell.Current).GoToFavorites();
        
        return true;
    }

    private async void ButtonAbout_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }
}
