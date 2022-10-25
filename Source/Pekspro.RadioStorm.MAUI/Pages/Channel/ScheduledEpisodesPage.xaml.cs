namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

[QueryProperty(nameof(Data), nameof(Data))]
public sealed partial class ScheduledEpisodesPage : ContentPage
{
    public int Data { get; set; }
    
    public ScheduledEpisodesPage(SchedulesEpisodesViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }

    private SchedulesEpisodesViewModel ViewModel => (SchedulesEpisodesViewModel) BindingContext;

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
