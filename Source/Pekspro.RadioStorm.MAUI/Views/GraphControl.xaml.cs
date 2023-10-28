namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class GraphControl
{
    public GraphControl()
    {
        InitializeComponent();

        if (ServiceProviderHelper.Current is not null)
        {
            var s = ServiceProviderHelper.GetRequiredService<GraphViewModel>();

            BindingContext = s;
        }
    }

    private GraphViewModel ViewModel => (GraphViewModel)BindingContext;
}
