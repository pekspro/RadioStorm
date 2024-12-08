namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public sealed partial class ChannelSongListPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

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
