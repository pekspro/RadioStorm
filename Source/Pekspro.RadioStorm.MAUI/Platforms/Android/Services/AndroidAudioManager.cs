namespace Pekspro.RadioStorm.MAUI.Services;

internal class AndroidAudioManager : AudioManagerBase
{
    MainActivity instance;
    private Microsoft.NetConf2021.Maui.Platforms.Android.Services.MediaPlayerService MediaPlayerService
        => this.instance.binder.GetMediaPlayerService();

    bool EventsAreSetup = false;

    public AndroidAudioManager(
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        IListenStateManager listenStateManager,
        IRecentPlayedManager recentPlayedManager,
        IDownloadManager downloadManager,
        ILocalSettings localSettings,
        IMessenger messenger,
        ILogger<AndroidAudioManager> logger)
        : base(mainThreadTimerFactory, mainThreadRunner, listenStateManager, recentPlayedManager, downloadManager, localSettings, messenger, logger, false)
    {

    }

    protected async override void MediaPlay(PlayListItem playlistItem)
    {
        try
        {
            if (this.instance is null)
            {
                this.instance = MainActivity.instance;
            }
            else
            {
                await MediaPlayerService.Stop();
            }

            var service = MediaPlayerService;

            SetupEvents(service);

            service.Item = playlistItem;
            service.AudioUrl = playlistItem.PreferablePlayUrl;

            await service.Play();
            // await service.Seek(pos * 1000);
        }
        catch (Exception )
        {

        }
    }

    protected override void MediaPlay()
    {
        _ = MediaPlayerService.Play();
    }

    protected override void MediaPause()
    {
        _ = MediaPlayerService.Pause();
    }

    protected override void MediaSetPlaybackPosition(TimeSpan position)
    {
        // throw new NotImplementedException();
        int pos = (int)position.TotalMilliseconds;
        int mediaDuration = MediaPlayerService.Duration;

        if (pos < mediaDuration)
        {
            _ = MediaPlayerService.Seek(pos);

            SetPositionAndLength(TimeSpan.FromMilliseconds(pos), TimeSpan.FromMilliseconds(mediaDuration));
        }
    }

    protected override void MediaRefreshButtonStates()
    {
        bool canPlay = false;
        bool canPause = false;
        bool isBuffering = false;

        var currentState = MediaPlayerService.MediaPlayerState;

        if (currentState == Android.Media.Session.PlaybackStateCode.Playing)
        {
            canPlay = false;
            canPause = true;
        }
        else if (currentState == Android.Media.Session.PlaybackStateCode.Paused)
        {
            canPlay = true;
            canPause = false;
        }
        else if (currentState == Android.Media.Session.PlaybackStateCode.Stopped)
        {
            canPlay = true;
            canPause = false;
        }
        else if (currentState == Android.Media.Session.PlaybackStateCode.Buffering || currentState == Android.Media.Session.PlaybackStateCode.Connecting)
        {
            canPlay = false;
            canPause = true;
            isBuffering = true;
        }
        else
        {
            canPlay = true;
            canPause = false;
        }

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
            //var duration = mediaPlayer.PlaybackSession.NaturalDuration;
            //if (duration.TotalSeconds <= 0)
            //{
            //    SetLiveAudioPositionAndLength();
            //    return;
            //}

            var position = TimeSpan.FromMilliseconds(MediaPlayerService.Position);
            var mediaLength = TimeSpan.FromMilliseconds(MediaPlayerService.Duration);

            SetPositionAndLength(position, mediaLength);
        }
    }

    protected override void MediaSetVolume(int volume)
    {
        // mediaPlayer .Volume = volume * 0.01;
    }

    public override bool HasVolumeSupport => false;

    private void SetupEvents(Microsoft.NetConf2021.Maui.Platforms.Android.Services.MediaPlayerService service)
    {
        if (!EventsAreSetup)
        {
            EventsAreSetup = true;
            service.StatusChanged += (a, b) => RefreshState();
        }
    }
}
