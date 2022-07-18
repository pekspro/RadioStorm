namespace Pekspro.RadioStorm.MAUI.Views;

public partial class GraphControl
{
    public GraphControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<GraphViewModel>();

            BindingContext = s;
        }
    }

    protected GraphViewModel ViewModel => (GraphViewModel)BindingContext;
}
