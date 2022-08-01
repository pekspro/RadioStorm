using Pekspro.RadioStorm.MAUI.Pages.Logging;

namespace Pekspro.RadioStorm.MAUI.Pages.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected SettingsViewModel ViewModel => (SettingsViewModel) BindingContext;

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        DebugSettings.OnNavigatedTo();
    }
}
