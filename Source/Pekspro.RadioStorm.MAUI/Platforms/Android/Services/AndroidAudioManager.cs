﻿namespace Pekspro.RadioStorm.MAUI.Services;

#nullable disable

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

            await service.Play(playlistItem.PreferablePlayUrl);
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
            };
        }
    }
}
