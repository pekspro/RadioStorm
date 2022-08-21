namespace Pekspro.RadioStorm.MAUI.Pages.Channel;

public partial class ChannelsPage : ContentPage
{
    public ChannelsPage(ChannelsViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }

    protected ChannelsViewModel ViewModel => (ChannelsViewModel) BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ViewModel.OnNavigatedTo();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        ViewModel.OnNavigatedFrom();
    }

    async void ChannelTapped(object sender, EventArgs e)
    {
        if((sender as ChannelControl)?.BindingContext is ChannelModel channel)
        {
            string param = ChannelDetailsViewModel.CreateStartParameter(channel);

            await Shell.Current.GoToAsync(nameof(ChannelDetailsPage), new Dictionary<string, object>()
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
}
