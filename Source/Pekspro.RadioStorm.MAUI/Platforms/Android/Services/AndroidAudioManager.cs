namespace Pekspro.RadioStorm.MAUI.Services;

#nullable disable

internal class AndroidAudioManager : AudioManagerBase
{
    MainActivity instance;
    private Pekspro.RadioStorm.MAUI.Platforms.Android.Services.MediaPlayerService MediaPlayerService
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
        IDateTimeProvider dateTimeProvider,
        ILogger<AndroidAudioManager> logger)
        : base(mainThreadTimerFactory, mainThreadRunner, listenStateManager, recentPlayedManager, downloadManager, localSettings, messenger, dateTimeProvider, logger, false)
    {

    }

    protected async override void MediaPlay(PlayList playlist)
    {
        try
        {
            if (this.instance is null)
            {
                this.instance = MainActivity.instance;
            }

            var service = MediaPlayerService;

            SetupEvents(service);

            await service.Play(playlist);
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
            // Only check position when playing or paused. Otherwise position
            // could be wrong. Especially likely when switching to a new item
            // in the play list.
            if (MediaPlayerService.MediaPlayerState is
                Android.Media.Session.PlaybackStateCode.Playing or
                Android.Media.Session.PlaybackStateCode.Paused
                )
            {
                var position = MediaPlayerService.Position;
                var mediaLength = MediaPlayerService.Duration;

                // Position are not valid is some situations (for instances, when stopped).
                if (position >= 0 && mediaLength > 0)
                {
                    SetPositionAndLength(TimeSpan.FromMilliseconds(position), TimeSpan.FromMilliseconds(mediaLength));
                }
            }
        }
    }

    protected override void MediaSetVolume(double volume)
    {
        if (instance is not null)
        {
            MediaPlayerService?.SetVolume((float)volume);
        }
    }

    protected override void OnPlayListChanged()
    {
        MediaPlayerService?.UpdateNotification();
    }

    private void SetupEvents(Pekspro.RadioStorm.MAUI.Platforms.Android.Services.MediaPlayerService service)
    {
        if (!EventsAreSetup)
        {
            EventsAreSetup = true;
            service.StatusChanged += (a, b) =>
            {
                RefreshState();

                var currentState = MediaPlayerService.MediaPlayerState;

                if (currentState == Android.Media.Session.PlaybackStateCode.Playing)
                {
                    TryRestorePosition(MediaLength);
                }
                else if (currentState == Android.Media.Session.PlaybackStateCode.SkippingToNext)
                {
                    if (!GoToNext())
                    {
                        Pause();
                    }
                }
                else if (currentState == Android.Media.Session.PlaybackStateCode.SkippingToPrevious)
                {
                    if (!GoToPrevious())
                    {
                        Pause();
                    }
                }
                else if (currentState == Android.Media.Session.PlaybackStateCode.Stopped)
                {
                    Logger.LogInformation("Player stopped. Will restore position on next play.");
                    RestorePositionOnNextPlay();
                }
            };
        }
    }

    protected override void SetPlaybackRate(double playbackRate)
    {
        MediaPlayerService?.SetPlaybackRate(playbackRate);
    }
}
