namespace Pekspro.RadioStorm.MAUI.Pages.Program;

[QueryProperty(nameof(Data), nameof(Data))]
public partial class ProgramSettingsPage : ContentPage
{
    public string Data { get; set; }
    
    public ProgramSettingsPage(ProgramSettingsViewModel viewModel)
	{
		InitializeComponent();
        
        BindingContext = viewModel;
    }

    protected ProgramSettingsViewModel ViewModel => BindingContext as ProgramSettingsViewModel;

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
