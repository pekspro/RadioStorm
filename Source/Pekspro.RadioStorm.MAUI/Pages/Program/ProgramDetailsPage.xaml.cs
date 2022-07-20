﻿namespace Pekspro.RadioStorm.MAUI.Pages.Program;

[QueryProperty(nameof(Data), nameof(Data))]
public partial class ProgramDetailsPage : ContentPage
{
    public string Data { get; set; }

    public ProgramDetailsPage(ProgramDetailsViewModel viewModel)
    {
        InitializeComponent();
        
        WidthStateHelper.ConfigureWidthState(GridLayout, this);

        BindingContext = viewModel;
    }

    protected ProgramDetailsViewModel ViewModel => BindingContext as ProgramDetailsViewModel;

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

    async private void EpisodeTapped(object sender, EventArgs e)
    {
        if ((sender as EpisodeControl)?.BindingContext is EpisodeModel episode)
        {
            string param = EpisodeDetailsViewModel.CreateStartParameter(episode, false);

            await Shell.Current.GoToAsync(nameof(EpisodeDetailsPage), new Dictionary<string, object>()
            {
                { "Data", param }
            });
        }
    }
    
    async private void ToolbarItemSettings_Clicked(object sender, EventArgs e)
    {
        string param = ProgramSettingsViewModel.CreateStartParameter(ViewModel.ProgramData);

        await Shell.Current.GoToAsync(nameof(ProgramSettingsPage), new Dictionary<string, object>()
        {
            { "Data", param }
        });
    }

    private void ButtonScrollToFirstNotListened_Clicked(object sender, EventArgs e)
    {
        int? position = ViewModel.EpisodesViewModel.FirstNotListenedEpisodePosition;

        if (position is not null)
        {
            EpisodesListView.ScrollTo(position.Value, position: ScrollToPosition.Center);
        }
    }
}
