namespace Pekspro.RadioStorm.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }
}
