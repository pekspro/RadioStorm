namespace Pekspro.RadioStorm.Sandbox.WPF;

public sealed partial class ChannelDetailsWindow : Window
{
    public ChannelDetailsWindow(ChannelDetailsViewModel channelInfoViewModel)
    {
        InitializeComponent();

        StartParameter = ChannelDetailsViewModel.CreateStartParameter(132, null, null, null);

        DataContext = channelInfoViewModel;
    }

    private ChannelDetailsViewModel ViewModel => (ChannelDetailsViewModel) DataContext;

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
