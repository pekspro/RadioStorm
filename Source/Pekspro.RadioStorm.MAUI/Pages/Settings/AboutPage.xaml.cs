namespace Pekspro.RadioStorm.MAUI.Pages.Settings;

public sealed partial class AboutPage : ContentPage
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private AboutViewModel ViewModel => (AboutViewModel)BindingContext;

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        DebugSettings.OnNavigatedTo();
    }
}