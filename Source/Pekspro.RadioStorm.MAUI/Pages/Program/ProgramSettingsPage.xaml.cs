namespace Pekspro.RadioStorm.MAUI.Pages.Program;

[QueryProperty(nameof(Data), nameof(Data))]
public sealed partial class ProgramSettingsPage : ContentPage
{
    public string Data { get; set; } = null!;

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
