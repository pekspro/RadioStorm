namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class DownloadSettingsControl
{
    public DownloadSettingsControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<DownloadSettingsViewModel>();

            BindingContext = s;
        }
    }
    
    private DownloadSettingsViewModel ViewModel => (DownloadSettingsViewModel)BindingContext;
}
