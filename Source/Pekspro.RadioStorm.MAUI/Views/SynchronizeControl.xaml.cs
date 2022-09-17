namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class SynchronizeControl
{
    public SynchronizeControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<SynchronizingViewModel>();

            BindingContext = s;
        }
    }

    private SynchronizingViewModel ViewModel => (SynchronizingViewModel)BindingContext;
}
