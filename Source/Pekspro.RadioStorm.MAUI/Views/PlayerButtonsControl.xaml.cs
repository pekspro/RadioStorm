namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class PlayerButtonsControl
{
    public PlayerButtonsControl()
    {
        InitializeComponent();

        if (ServiceProviderHelper.Current is not null)
        {
            var s = ServiceProviderHelper.GetRequiredService<PlayerViewModel>();

            BindingContext = s;
        }
    }

    private PlayerViewModel ViewModel => (PlayerViewModel) BindingContext;
}
