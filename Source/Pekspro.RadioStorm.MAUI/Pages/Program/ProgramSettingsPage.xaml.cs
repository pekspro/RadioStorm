namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public sealed partial class ProgramSettingsPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

    public ProgramSettingsPage(ProgramSettingsViewModel viewModel)
	{
		InitializeComponent();
        
        BindingContext = viewModel;
    }

    private ProgramSettingsViewModel ViewModel => (ProgramSettingsViewModel) BindingContext;

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
