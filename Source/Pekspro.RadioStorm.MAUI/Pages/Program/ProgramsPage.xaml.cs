namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public partial class ProgramsPage : ContentPage
{
    public ProgramsPage(ProgramsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected ProgramsViewModel ViewModel => BindingContext as ProgramsViewModel;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo();
    }

    async private void ProgramTapped(object sender, EventArgs e)
    {
        if ((sender as ProgramControl)?.BindingContext is ProgramModel program)
        {
            string param = ProgramDetailsViewModel.CreateStartParameter(program);

            await Shell.Current.GoToAsync(nameof(ProgramDetailsPage), new Dictionary<string, object>()
            {
                { "Data", param }
            });
        }
    }
}
