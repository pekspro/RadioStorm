namespace Pekspro.RadioStorm.MAUI.Pages.Program;

public sealed partial class ProgramsPage : ContentPage
{
    public ProgramsPage(ProgramsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private ProgramsViewModel ViewModel => (ProgramsViewModel) BindingContext;

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

    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        SwipeHelper.SwipeStarted(sender);
    }

    private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        SwipeHelper.SwipeEnded(sender);
    }

    protected override bool OnBackButtonPressed()
    {
        ((AppShell)Shell.Current).GoToFavorites();

        return true;
    }
}
