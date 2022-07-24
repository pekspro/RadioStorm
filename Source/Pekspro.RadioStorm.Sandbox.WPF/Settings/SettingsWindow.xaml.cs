using System.Diagnostics;

namespace Pekspro.RadioStorm.Sandbox.WPF.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel settingsViewModel, DebugSettingsViewModel debugSettingsViewModel)
    {
        InitializeComponent();
        DataContext = settingsViewModel;
        DebugSettingsPanel.DataContext = debugSettingsViewModel;

        debugSettingsViewModel.OnZipFileCreated = (x) =>
        {
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", x));
        };
    }

    protected SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
    
    protected DebugSettingsViewModel DebugViewModel => (DebugSettingsViewModel) DebugSettingsPanel.DataContext;

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        DebugViewModel.OnNavigatedTo();

        // ViewModel.OnNavigatedTo();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        // ViewModel.OnNavigatedFrom();
    }
}
