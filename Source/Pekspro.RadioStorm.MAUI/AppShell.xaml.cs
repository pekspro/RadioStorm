namespace Pekspro.RadioStorm.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Current.GoToAsync(nameof(SettingsPage));

        if (Current.FlyoutBehavior == FlyoutBehavior.Flyout)
        {
            Current.FlyoutIsPresented = false;
        }
    }
}
