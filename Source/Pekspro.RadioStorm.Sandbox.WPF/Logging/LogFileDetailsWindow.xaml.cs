namespace Pekspro.RadioStorm.Sandbox.WPF.LogFile;

public partial class LogFileDetailsWindow : Window
{
    public LogFileDetailsWindow(LogFileDetailsViewModel LogFileDetailsViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        StartParameter = LogFileDetailsViewModel.CreateStartParameter("XYZ");

        DataContext = LogFileDetailsViewModel;
        ServiceProvider = serviceProvider;
    }

    private LogFileDetailsViewModel ViewModel => (LogFileDetailsViewModel)DataContext;

    public string StartParameter { get; set; }
    public IServiceProvider ServiceProvider { get; }

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
