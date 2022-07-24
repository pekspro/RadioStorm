namespace Pekspro.RadioStorm.MAUI.Pages.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel, AboutViewModel aboutViewModel, DebugSettingsViewModel debugSettingsViewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        AboutLayout.BindingContext = aboutViewModel;
        DebugSettings.BindingContext = debugSettingsViewModel;

#if WINDOWS
        debugSettingsViewModel.OnZipFileCreated = (x) =>
        {
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", x));
        };
#endif

#if ANDROID
        LogEditorBox.Text = Microsoft.NetConf2021.Maui.Platforms.Android.Services.MaintenanceJobService.GetLog().ReplaceLineEndings();
#endif
    }

    protected SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

    protected DebugSettingsViewModel DebugSettingsViewModel => DebugSettings.BindingContext as DebugSettingsViewModel;

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        DebugSettingsViewModel.OnNavigatedTo();
    }

    private readonly List<DateTime> TapTimestamps = new();

    private void VersionNumberLabel_Tapped(object sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        TapTimestamps.Add(now);

        // If more than 5 taps within latest 10 seconds
        if (TapTimestamps.Count(a => a >= now.AddSeconds(-10)) > 5)
        {
            TapTimestamps.Clear();
            DebugSettingsViewModel.ShowDebugSettings = !DebugSettingsViewModel.ShowDebugSettings;
        }
    }

    private void OnReadLogFileClicked(object sender, EventArgs e)
    {
#if ANDROID
        LogEditorBox.Text = Microsoft.NetConf2021.Maui.Platforms.Android.Services.MaintenanceJobService.GetLog()
            .ReplaceLineEndings();
#endif
    }

    private void OnClearLogFileClicked(object sender, EventArgs e)
    {
#if ANDROID
        Microsoft.NetConf2021.Maui.Platforms.Android.Services.MaintenanceJobService.ClearLog();
        LogEditorBox.Text = string.Empty;
#endif
    }
}
