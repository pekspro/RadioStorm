namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

[QueryProperty(nameof(Data), nameof(Data))]
public partial class ChannelSongListPage : ContentPage
{
    public string Data { get; set; } = null!;

    public ChannelSongListPage(SongsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    private SongsViewModel ViewModel => (SongsViewModel)BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo(true, Data);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }
}
