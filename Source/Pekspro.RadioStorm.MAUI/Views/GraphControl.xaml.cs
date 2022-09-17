namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class GraphControl
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

    private GraphViewModel ViewModel => (GraphViewModel)BindingContext;
}
