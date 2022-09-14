namespace Pekspro.RadioStorm.MAUI.Pages.Logging;

[QueryProperty(nameof(Data), nameof(Data))]
public partial class LogFileDetailsPage : ContentPage
{
    public string Data { get; set; } = null!;

    public LogFileDetailsPage(LogFileDetailsViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
	}

    private LogFileDetailsViewModel ViewModel => (LogFileDetailsViewModel) BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo(Data);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }
}
