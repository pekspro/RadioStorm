namespace Pekspro.RadioStorm.Sandbox.WPF.Program;

/// <summary>
/// Interaction logic for ProgramSettingsWindow.xaml
/// </summary>
public partial class ProgramSettingsWindow : Window
{
    public ProgramSettingsWindow(ProgramSettingsViewModel programSettingsViewModel)
    {
        InitializeComponent();

        StartParameter = ProgramSettingsViewModel.CreateStartParameter(132, "XYZ");

        DataContext = programSettingsViewModel;
    }

    protected ProgramSettingsViewModel ViewModel => (ProgramSettingsViewModel)DataContext;

    public string StartParameter { get; set; }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        ViewModel.OnNavigatedTo(StartParameter);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        ViewModel.OnNavigatedFrom();
    }
}
