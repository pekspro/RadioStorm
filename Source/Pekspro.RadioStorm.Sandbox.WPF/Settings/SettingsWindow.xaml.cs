using System.Diagnostics;
using Pekspro.RadioStorm.Sandbox.WPF.LogFile;

namespace Pekspro.RadioStorm.Sandbox.WPF.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel settingsViewModel, DebugSettingsViewModel debugSettingsViewModel, 
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = settingsViewModel;
        DebugSettingsPanel.DataContext = debugSettingsViewModel;

        debugSettingsViewModel.OnZipFileCreated = (x) =>
        {
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", x));
        };
        ServiceProvider = serviceProvider;
    }
    
    protected IServiceProvider ServiceProvider { get; }

    protected SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
    
    protected DebugSettingsViewModel DebugSettingsViewModel => (DebugSettingsViewModel) DebugSettingsPanel.DataContext;

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        DebugSettingsViewModel.OnNavigatedTo();

        // ViewModel.OnNavigatedTo();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        // ViewModel.OnNavigatedFrom();
    }

    private void ButtonOpenLogFile_Click(object sender, RoutedEventArgs e)
    {
        var logFilePath = DebugSettingsViewModel.SelectedLogFilePath;

        var logDetailsWindow = ServiceProvider.GetRequiredService<LogFileDetailsWindow>();
        logDetailsWindow.StartParameter = LogFileDetailsViewModel.CreateStartParameter(logFilePath);
        logDetailsWindow.Show();
    }
}
