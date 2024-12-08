namespace Pekspro.RadioStorm.MAUI.Pages.Logging;

public sealed partial class LogFileDetailsPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

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
