namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

/// <summary>
/// Interaction logic for DebugSettingsControl.xaml
/// </summary>
public sealed partial class DebugSettingsControl : UserControl
{
    FakeConnectivityProvider ConnectivityProvider;
    
    public DebugSettingsControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            ConnectivityProvider = App.ServiceProvider.GetRequiredService<IConnectivityProvider>() as FakeConnectivityProvider;

            CheckBoxInternetAccess.IsChecked = ConnectivityProvider.HasInternetAccess;
        }
    }

    private void CheckBoxInternetAccess_Checked(object sender, RoutedEventArgs e)
    {
        ConnectivityProvider.HasInternetAccess = CheckBoxInternetAccess.IsChecked == true;
    }
}
