namespace Pekspro.RadioStorm.MAUI.Pages.Settings;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel, AboutViewModel aboutViewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
        AboutLayout.BindingContext = aboutViewModel;

#if ANDROID
        LogEditorBox.Text = Microsoft.NetConf2021.Maui.Platforms.Android.Services.MaintenanceJobService.GetLog().ReplaceLineEndings();
#endif
    }

    protected SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

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
