﻿namespace Pekspro.RadioStorm.MAUI.Views;

public sealed partial class PlayerControl
{
    public PlayerControl()
    {
        InitializeComponent();

        if (Services.ServiceProvider.Current is not null)
        {
            var s = Services.ServiceProvider.GetRequiredService<CurrentPlayingViewModel>();

            BindingContext = s;
        }
    }

    private CurrentPlayingViewModel ViewModel => (CurrentPlayingViewModel) BindingContext;

    private async void ButtonPlayerInfo_Clicked(object sender, EventArgs e)
    {
        // Open CurrentPlayingPage, or go back if already open
        if (Shell.Current.CurrentPage is CurrentPlayingPage)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(CurrentPlayingPage));
        }
    }
}
