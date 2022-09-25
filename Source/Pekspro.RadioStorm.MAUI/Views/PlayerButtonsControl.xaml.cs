namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class PlayerButtonsControl
{
    public PlayerButtonsControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<PlayerViewModel>();

            BindingContext = s;
        }
    }

    private PlayerViewModel ViewModel => (PlayerViewModel) BindingContext;
}
