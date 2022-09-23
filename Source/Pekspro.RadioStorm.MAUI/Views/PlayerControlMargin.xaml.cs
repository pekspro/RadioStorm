namespace Pekspro.RadioStorm.MAUI.Views;

public partial class PlayerControlMargin : ContentView
{
	public PlayerControlMargin()
	{
		InitializeComponent();
        if (Services.ServiceProvider.Current is not null)

        {
            var s = Services.ServiceProvider.GetRequiredService<PlayerViewModel>();

            BindingContext = s;
        }
    }

    private PlayerViewModel ViewModel => (PlayerViewModel)BindingContext;
}
