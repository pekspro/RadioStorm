using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace Pekspro.RadioStorm.MAUI.Services;

sealed class WindowsAudioManager : AudioManagerBase
{
    MediaPlayer mediaPlayer = new MediaPlayer();
    
    public WindowsAudioManager(
        IMainThreadTimerFactory mainThreadTimerFactory,
        IMainThreadRunner mainThreadRunner,
        IListenStateManager listenStateManager,
        IRecentPlayedManager recentPlayedManager,
        IDownloadManager downloadManager,
        ILocalSettings localSettings,
        IMessenger messenger,
        IDateTimeProvider dateTimeProvider,
        ILogger<WindowsAudioManager> logger)
        : base(mainThreadTimerFactory, mainThreadRunner, listenStateManager, recentPlayedManager, downloadManager, localSettings, messenger, dateTimeProvider, logger, false)
    {
        mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
        mediaPlayer.PlaybackSession.PlaybackStateChanged += (a, b) =>
        {
            RefreshState();
            var s = a.PlaybackState;

            switch (a.PlaybackState)
            {
                case MediaPlaybackState.Buffering:
                case MediaPlaybackState.Playing:
                {
                    var duration = mediaPlayer.NaturalDuration;
                    TryRestorePosition(duration);
                    break;
                }
            }
        };

        mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
        mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;
    }

    private int BufferingSessionId = 0;

    private async void PlaybackSession_BufferingStarted(MediaPlaybackSession sender, object args)
    {
        Logger.LogWarning(nameof(PlaybackSession_BufferingStarted));

        BufferingSessionId++;

        int buffereingSessionId = BufferingSessionId;

        while (buffereingSessionId == BufferingSessionId)
        {
            try
            {
                BufferRatio = mediaPlayer.PlaybackSession.BufferingProgress;
            }
            catch(Exception )
            {
                // I have seen at least one cast exception here.
            }

            await Task.Delay(1);
        }
    }

    private void PlaybackSession_BufferingEnded(MediaPlaybackSession sender, object args)
    {
        Logger.LogWarning(nameof(PlaybackSession_BufferingEnded));

        BufferingSessionId++;
        BufferRatio = mediaPlayer.PlaybackSession.BufferingProgress;
    }

    protected override void MediaPlay(PlayList playlist)
    {
        BufferingSessionId++;
        var playbackItem = CreateMediaPlaybackItem(playlist.CurrentItem!);
        mediaPlayer.Source = playbackItem;

        MediaPlay();
    }

    protected override void MediaPlay()
    {
        mediaPlayer.Play();
    }

    protected override void MediaPause()
    {
        mediaPlayer.Pause();
    }

    protected override void MediaSetPlaybackPosition(TimeSpan position)
    {
        if (position < mediaPlayer.NaturalDuration)
        {
            mediaPlayer.Position = position;
        }
    }

    protected override void MediaRefreshButtonStates()
    {
        bool canPlay = false;
        bool canPause = true;
        var currentState = mediaPlayer.CurrentState;

        if (currentState == MediaPlayerState.Playing)
        {
            canPlay = false;
            canPause = true;
        }
        else if (currentState == MediaPlayerState.Paused)
        {
            canPlay = true;
            canPause = false;
        }
        else if (currentState == MediaPlayerState.Stopped)
        {
            canPlay = true;
            canPause = false;
        }
        /*else if (currentState == MediaPlaybackState.Stopped)
        {
            CanPlay = true;
            CanPause = false;
        }
        else if (currentState == MediaPlaybackState.Closed)
        {
            CanPlay = true;
            CanPause = false;
        }*/
        else if (currentState == MediaPlayerState.Buffering || currentState == MediaPlayerState.Opening)
        {
            canPlay = false;
            canPause = true;
        }

        bool isBuffering = (currentState == MediaPlayerState.Buffering || currentState == MediaPlayerState.Opening);

        UpdateButtonStates(canPlay, canPause, isBuffering);
    }

    protected override void MediaRefreshLengthAndPosition()
    {
        if (mediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing &&
            mediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Paused)
        {
            return;
        }

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
            var duration = mediaPlayer.PlaybackSession.NaturalDuration;
            if (duration.TotalSeconds <= 0)
            {
                SetLiveAudioPositionAndLength();
                return;
            }

            var position = mediaPlayer.PlaybackSession.Position; ;
            var mediaLength = mediaPlayer.PlaybackSession.NaturalDuration;

            SetPositionAndLength(position, mediaLength);
        }
    }

    protected override void MediaSetVolume(double volume)
    {
        mediaPlayer.Volume = volume;
    }

    private static MediaPlaybackItem CreateMediaPlaybackItem(PlayListItem song)
    {
        if (song.PreferablePlayUrl is null)
        {
            throw new NullReferenceException(nameof(song.PreferablePlayUrl));
        }

        var source = MediaSource.CreateFromUri(new Uri(song.PreferablePlayUrl));
        source.CustomProperties[nameof(PlayListItem)] = song;
        var mediaItem = new MediaPlaybackItem(source);
        var displayProperties = mediaItem.GetDisplayProperties();

        displayProperties.Type = Windows.Media.MediaPlaybackType.Music;

        if (song.IsLiveAudio)
        {
            displayProperties.MusicProperties.Title = song.Channel ?? "";
            displayProperties.MusicProperties.Artist = string.Empty;
        }
        else
        {
            displayProperties.MusicProperties.Title = song.Program ?? "";
            displayProperties.MusicProperties.Artist = song.Episode ?? "";
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(song.IconUri))
            {
                displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(song.IconUri));
            }
        }
        catch (Exception)
        {
        }

        mediaItem.ApplyDisplayProperties(displayProperties);
        return mediaItem;
    }

    protected override void SetPlaybackRate(double playbackRate)
    {
        mediaPlayer.PlaybackRate = playbackRate;
    }
}
