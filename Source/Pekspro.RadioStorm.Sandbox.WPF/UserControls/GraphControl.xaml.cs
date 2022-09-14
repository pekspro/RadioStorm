namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

/// <summary>
/// Interaction logic for GraphControl.xaml
/// </summary>
public partial class GraphControl : UserControl
{
    public GraphControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<GraphViewModel>();
        }
    }

    private GraphViewModel ViewModel => (GraphViewModel)DataContext;
}
