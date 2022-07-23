namespace Pekspro.RadioStorm.Sandbox.WPF.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel settingsViewModel, DebugSettingsViewModel debugSettingsViewModel)
    {
        InitializeComponent();
        DataContext = settingsViewModel;
        DebugSettingsPanel.DataContext = debugSettingsViewModel;
    }

    protected SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        // ViewModel.OnNavigatedTo();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        // ViewModel.OnNavigatedFrom();
    }
}
