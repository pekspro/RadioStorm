using Pekspro.RadioStorm.Sandbox.WPF.Player;

namespace Pekspro.RadioStorm.Sandbox.WPF.UserControls;

public sealed partial class PlayerControl : UserControl
{
    public PlayerControl()
    {
        InitializeComponent();

        if (App.ServiceProvider is not null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<PlayerViewModel>();
            SliderVolume.Value = (int) ((App.ServiceProvider.GetRequiredService<IAudioManager>() as WpfAudioManager).MediaPlayer.Volume * 100);
        }
    }

    private PlayerViewModel ViewModel => (PlayerViewModel)DataContext;

    private void ButtonOpenPlaylist_Click(object sender, RoutedEventArgs e)
    {
        var playListWindow = App.ServiceProvider.GetRequiredService<PlaylistWindow>();
        playListWindow.Show();
    }

    private void ContextMenuVerySlow_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSpeedVerySlow();
    }

    private void ContextMenuSlow_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSpeedSlow();
    }

    private void ContextMenuNormal_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSpeedNormal();
    }

    private void ContextMenuFast_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSpeedFast();
    }

    private void ContextMenuVeryFast_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SetSpeedVeryFast();
    }
}
