namespace Pekspro.RadioStorm.MAUI.Pages.Episode;

public partial class EpisodeSongListPage : ContentPage, IQueryAttributable
{
    public string Data { get; set; } = null!;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Data), out var data) && data is not null)
        {
            Data = data.ToString()!;
        }
    }

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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }
}
