namespace Pekspro.RadioStorm.MAUI.Pages.Episode;

[QueryProperty(nameof(Data), nameof(Data))]
public partial class EpisodeSongListPage : ContentPage
{
    public string Data { get; set; } = null!;

    public EpisodeSongListPage(SongsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private SongsViewModel ViewModel => (SongsViewModel)BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo(false, Data);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        this.FixToolbarItems();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }
}
