﻿namespace Pekspro.RadioStorm.MAUI.Services;

#nullable enable

internal sealed class AndroidAudioManager : AudioManagerBase
{
    private Platforms.Android.Services.MediaPlayerService? _MediaPlayerService;
    
    internal Platforms.Android.Services.MediaPlayerService? MediaPlayerService
    {
        get
        {
            return _MediaPlayerService;
        }
        set
        {
            _MediaPlayerService = value;

            if (_MediaPlayerService is not null)
            {
                SetupEvents(_MediaPlayerService);
            }
        }
    }

    internal Action<Action<Platforms.Android.Services.MediaPlayerService>?>? MediaServiceStarter;

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
            if (MediaPlayerService is not null)
            {
                await MediaPlayerService.Play(playlist);
            }
            else
            {
                MediaServiceStarter?.Invoke((service) =>
                {
                    service.Play(playlist);
                    RefreshState();
                });
            }
        }
        catch (Exception )
        {

        }
    }

    protected override void MediaPlay()
    {
        MediaPlayerService?.Play();
    }

    protected override void MediaPause()
    {
        MediaPlayerService?.Pause();
    }

    protected override void MediaStop()
    {
        MediaPlayerService?.Stop(false);
    }

    protected override void MediaSetPlaybackPosition(TimeSpan position)
    {
        if (MediaPlayerService is null)
        {
            return;
        }

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
        if (MediaPlayerService is null)
        {
            return;
        }
        
        bool canPlay;
        bool canPause;
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
            if (MediaPlayerService is null)
            {
                return;
            }
            
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
        if (_MediaPlayerService is not null)
        {
            MediaPlayerService?.SetVolume((float)volume);
        }
    }

    protected override void OnPlayListChanged()
    {
        MediaPlayerService?.UpdateNotification();
    }

    private void SetupEvents(Platforms.Android.Services.MediaPlayerService service)
    {
        service.StatusChanged += (a, b) =>
        {
            RefreshState();

            var currentState = service.MediaPlayerState;

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

        service.BufferChanged += (a, b) =>
        {
            if (MediaPlayerService is null)
            {
                return;
            }

            BufferRatio = MediaPlayerService.Buffered * 0.01;
        };
    }

    protected override void SetPlaybackRate(double playbackRate)
    {
        MediaPlayerService?.SetPlaybackRate(playbackRate);
    }
}
