namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

/// <summary>
/// Interaction logic for GraphStateControl.xaml
/// </summary>
public partial class GraphStateControl : UserControl
{
    public GraphStateControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<GraphViewModel>();
        }
    }

    private GraphViewModel ViewModel => (GraphViewModel)DataContext;
}
