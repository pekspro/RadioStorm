namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class DebugSettingsControl : ContentView
{
	public DebugSettingsControl()
	{
		InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var debugSettingsViewModel = Services.ServiceProvider.GetRequiredService<DebugSettingsViewModel>();

            BindingContext = debugSettingsViewModel;

#if WINDOWS
        debugSettingsViewModel.OnZipFileCreated = (x) =>
        {
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", x));
        };
#endif

#if ANDROID
            debugSettingsViewModel.OnZipFileCreated = async (x) =>
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = Pekspro.RadioStorm.UI.Resources.Strings.Settings_Debug_LogFiles_SendLogFiles,
                    File = new ShareFile(x)
                });
            };
#endif

        }
    }

    private DebugSettingsViewModel ViewModel => (DebugSettingsViewModel)BindingContext;

    public void OnNavigatedTo()
    {
        ViewModel.OnNavigatedTo();
    }

    private async void ButtonOpenLogFile_Clicked(object sender, EventArgs e)
    {
        var path = ViewModel.SelectedLogFilePath;

        if (path is not null)
        {
            string param = LogFileDetailsViewModel.CreateStartParameter(path);

            await Shell.Current.GoToAsync(nameof(LogFileDetailsPage), new Dictionary<string, object>()
                {
                    { "Data", param }
                });
        }
    }
}
