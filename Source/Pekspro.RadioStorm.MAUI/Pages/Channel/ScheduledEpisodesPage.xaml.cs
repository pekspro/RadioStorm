namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

[QueryProperty(nameof(Data), nameof(Data))]
public sealed partial class ScheduledEpisodesPage : ContentPage
{
    public string Data { get; set; } = null!;

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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

#if ANDROID
        // TODO: Remove when fixed: https://github.com/dotnet/maui/issues/10452

        ToolbarItemUpdate.IsVisible = false;

        Application.Current!.Dispatcher.Dispatch(() =>
        {
            ToolbarItemUpdate.IsVisible = true;
        });
#endif 
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }
}
