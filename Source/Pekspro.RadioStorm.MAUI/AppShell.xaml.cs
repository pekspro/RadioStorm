namespace Pekspro.RadioStorm.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    async protected override void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(2200);

        var color = FlyoutBackgroundColor;
        FlyoutBackgroundColor = Colors.DarkBlue;
        FlyoutBackgroundColor = color;
    }

    async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }
}
