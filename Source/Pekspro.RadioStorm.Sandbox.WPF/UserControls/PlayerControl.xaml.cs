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
}
