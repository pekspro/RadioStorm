namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class SynchronizeControl
{
    public SynchronizeControl()
    {
        InitializeComponent();

        if (ServiceProviderHelper.Current is not null)
        {
            var s = ServiceProviderHelper.GetRequiredService<SynchronizingViewModel>();

            BindingContext = s;
        }
    }

    private SynchronizingViewModel ViewModel => (SynchronizingViewModel)BindingContext;
}
