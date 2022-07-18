using Pekspro.RadioStorm.Audio.Models;
using Pekspro.RadioStorm.Settings;

namespace Pekspro.RadioStorm.Sandbox.WPF;

class WpfAudioManager : AudioManagerBase
{
    public readonly MediaPlayer MediaPlayer = new MediaPlayer();

    private bool IsPlaying = false;

    private TimeSpan? NextPositionToReport = null;

    public WpfAudioManager(
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        IListenStateManager listenStateManager,
        IRecentPlayedManager recentPlayedManager,
        IDownloadManager downloadManager,
        ILocalSettings localSettings,
        IMessenger messenger,
        ILogger<WpfAudioManager> logger)
        : base(mainThreadTimerFactory, mainThreadRunner, listenStateManager, recentPlayedManager, downloadManager, localSettings, messenger, logger, true)
    {
        MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
    }

    protected async override void MediaPlay(PlayListItem playlistItem)
    {
        MediaPlayer.Open(new Uri(playlistItem.PreferablePlayUrl));
        MediaPlay();

        for (int i = 0; i < 20; i++)
        {
            await System.Threading.Tasks.Task.Delay(100);
            RefreshState();

            if (CanPause)
            {
                break;
            }
        }
    }

    protected override void MediaPlay()
    {
        IsPlaying = true;
        // When playing starts after a pause, first position is often incorrect.
        // Cache current position and use that instead.
        NextPositionToReport = MediaPlayer.Position;

        MediaPlayer.Play();
        RefreshState();
    }

    protected override void MediaPause()
    {
        IsPlaying = false;
        MediaPlayer.Pause();
        RefreshState();
    }

    protected override void MediaSetPlaybackPosition(TimeSpan position)
    {
        MediaPlayer.Position = position;
    }

    protected override void MediaRefreshButtonStates()
    {
        bool isBuffering = MediaPlayer.IsBuffering;
        bool canPause = !isBuffering && MediaPlayer.CanPause && IsPlaying;
        bool canPlay = !isBuffering && !IsPlaying;

        UpdateButtonStates(canPlay, canPause, isBuffering);
    }

    protected override void MediaRefreshLengthAndPosition()
    {
        if (CurrentItem is null)
        {
            return;
        }

        if (CurrentItem.IsLiveAudio)
        {
            SetLiveAudioPositionAndLength();
        }
        else
        {
            if (!MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                SetLiveAudioPositionAndLength();
            }
            else
            {
                var duration = MediaPlayer.NaturalDuration.TimeSpan;
                SetPositionAndLength(NextPositionToReport ?? MediaPlayer.Position, duration);
            }
        }

        NextPositionToReport = null;
    }

    protected override void MediaSetVolume(int volume)
    {
        MediaPlayer.Volume = volume * 0.01;
    }

    public override bool HasVolumeSupport => true;

    private void MediaPlayer_MediaOpened(object sender, EventArgs e)
    {
        if (MediaPlayer.IsBuffering)
        {
            return;
        }

        if (!MediaPlayer.NaturalDuration.HasTimeSpan)
        {
            return;
        }
        else
        {
            var duration = MediaPlayer.NaturalDuration.TimeSpan;
            TryRestorePosition(duration);
        }

    }
}
