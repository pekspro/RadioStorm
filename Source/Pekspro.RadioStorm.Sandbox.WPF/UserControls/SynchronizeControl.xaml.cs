namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

public partial class SynchronizeControl : UserControl
{
    public SynchronizeControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<SynchronizingViewModel>();
        }
    }

    protected SynchronizingViewModel ViewModel => (SynchronizingViewModel)DataContext;
}
