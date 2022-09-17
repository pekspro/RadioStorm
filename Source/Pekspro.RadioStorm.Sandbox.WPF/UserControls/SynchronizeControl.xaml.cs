namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

public sealed partial class SynchronizeControl : UserControl
{
    public SynchronizeControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<SynchronizingViewModel>();
        }
    }

    private SynchronizingViewModel ViewModel => (SynchronizingViewModel)DataContext;
}
