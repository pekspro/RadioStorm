namespace Pekspro.RadioStorm.MAUI.Pages.Settings;

public sealed partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private SettingsViewModel ViewModel => (SettingsViewModel) BindingContext;

    private bool IsNavigatedTo = false;

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        ViewModel.RefreshNotificationSettings();

        IsNavigatedTo = true;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        IsNavigatedTo = false;
    }

    protected override bool OnBackButtonPressed()
    {
        ((AppShell)Shell.Current).GoToFavorites();
        
        return true;
    }

    private async void ButtonAbout_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    private void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (IsNavigatedTo)
        {
            (App.Current as Pekspro.RadioStorm.MAUI.App)?.ConfigureStatusBar();        
        }
    }
}
