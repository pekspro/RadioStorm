namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class DownloadSettingsControl
{
    public DownloadSettingsControl()
    {
        InitializeComponent();

        if (ServiceProviderHelper.Current is not null)
        {
            var s = ServiceProviderHelper.GetRequiredService<DownloadSettingsViewModel>();

            BindingContext = s;
        }
    }
    
    private DownloadSettingsViewModel ViewModel => (DownloadSettingsViewModel)BindingContext;
}
