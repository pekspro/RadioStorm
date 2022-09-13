namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

/// <summary>
/// Interaction logic for DownloadSettingsControl.xaml
/// </summary>
public partial class DownloadSettingsControl : UserControl
{
    public DownloadSettingsControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<DownloadSettingsViewModel>();
        }
    }

    private DownloadSettingsViewModel ViewModel => (DownloadSettingsViewModel) DataContext;
}
