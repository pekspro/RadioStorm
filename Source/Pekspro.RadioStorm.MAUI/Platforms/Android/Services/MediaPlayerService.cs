﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Java.Net;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Receivers;
using Pekspro.RadioStorm.MAUI;
using AndroidNet = Android.Net;

#nullable disable

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

[Service(Exported = true)]
[IntentFilter(new[] { ActionPlay, ActionPause, ActionStop, ActionTogglePlayback, ActionRewind, ActionForward, ActionNext, ActionPrevious })]
public class MediaPlayerService : Service,
   AudioManager.IOnAudioFocusChangeListener,
   MediaPlayer.IOnBufferingUpdateListener,
   MediaPlayer.IOnSeekCompleteListener,
   MediaPlayer.IOnCompletionListener,
   MediaPlayer.IOnErrorListener,
   MediaPlayer.IOnPreparedListener
{
    //Actions
    public const string ActionPlay = "com.pekspro.radiostorm.action.PLAY";
    public const string ActionPause = "com.pekspro.radiostorm.action.PAUSE";
    public const string ActionStop = "com.pekspro.radiostorm.action.STOP";
    public const string ActionTogglePlayback = "com.pekspro.radiostorm.action.TOGGLEPLAYBACK";
    public const string ActionRewind = "com.pekspro.radiostorm.action.REWIND";
    public const string ActionForward = "com.pekspro.radiostorm.action.FORWARD";
    public const string ActionNext = "com.pekspro.radiostorm.action.NEXT";
    public const string ActionPrevious = "com.pekspro.radiostorm.action.PREVIOUS";

    public MediaPlayer mediaPlayer;
    private AudioManager audioManager;

    private MediaSession mediaSession;
    public MediaController mediaController;

    private WifiManager wifiManager;
    private WifiManager.WifiLock wifiLock;

    public event StatusChangedEventHandler StatusChanged;

    private PlayList PlayList;
    private Bitmap ItemImage;
    private int AudioCounter;

    private ComponentName remoteComponentName;

    public PlaybackStateCode MediaPlayerState
    {
        get
        {
            return (mediaController.PlaybackState is not null
                ? mediaController.PlaybackState.State
                : PlaybackStateCode.None);
        }
    }

    public MediaPlayerService()
    {
    }

    private ILogger _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= Pekspro.RadioStorm.MAUI.Services.ServiceProvider.Current.GetRequiredService<ILogger<MediaPlayerService>>();
        }
    }

    protected virtual void OnStatusChanged(EventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }

    /// <summary>
    /// On create simply detect some of our managers
    /// </summary>
    public override void OnCreate()
    {
        base.OnCreate();

        Logger.LogInformation($"{nameof(OnCreate)}");

        //Find our audio and notificaton managers
        audioManager = (AudioManager)GetSystemService(AudioService);
        wifiManager = (WifiManager)GetSystemService(WifiService);

        remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);
    }

    /// <summary>
    /// Will register for the remote control client commands in audio manager
    /// </summary>
    private void InitMediaSession()
    {
        try
        {
            Logger.LogInformation(nameof(InitMediaSession));
            
            if (mediaSession is null)
            {
                Logger.LogInformation($"{nameof(InitMediaSession)} - Creating new session");
                Intent nIntent = new Intent(ApplicationContext, typeof(MainActivity));

                remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);

                mediaSession = new MediaSession(ApplicationContext, "MauiStreamingAudio");
                mediaSession.SetSessionActivity(PendingIntent.GetActivity(ApplicationContext, 0, nIntent, PendingIntentFlags.Mutable));
                mediaController = new MediaController(ApplicationContext, mediaSession.SessionToken);
            }

            mediaSession.Active = true;
            mediaSession.SetCallback(new MediaSessionCallback((MediaPlayerServiceBinder)binder));

            mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error in {nameof(InitMediaSession)}");
        }
    }

    /// <summary>
    /// Intializes the player.
    /// </summary>
    private void InitializePlayer()
    {
        Logger.LogInformation(nameof(InitializePlayer));
        
        mediaPlayer = new MediaPlayer();

        mediaPlayer.SetAudioAttributes(
            new AudioAttributes.Builder()
            .SetContentType(AudioContentType.Speech)
            .SetUsage(AudioUsageKind.Media)
                .Build());

        mediaPlayer.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);

        mediaPlayer.SetOnBufferingUpdateListener(this);
        mediaPlayer.SetOnSeekCompleteListener(this);
        mediaPlayer.SetOnCompletionListener(this);
        mediaPlayer.SetOnErrorListener(this);
        mediaPlayer.SetOnPreparedListener(this);
    }


    public void OnBufferingUpdate(MediaPlayer mp, int percent)
    {
        Logger.LogInformation("Buffering updated. Percent: {0}", percent);
        
        int duration = Duration;

        int newBufferedTime = duration * percent / 100;
        if (newBufferedTime != Buffered)
        {
            Buffered = newBufferedTime;
        }
    }

    public void OnSeekComplete(MediaPlayer mp)
    {
        UpdatePlaybackState(PlaybackStateCode.Playing);
    }

    public void OnCompletion(MediaPlayer mp)
    {
        Logger.LogInformation("Audio playing is completed.");

        PlayNext();

    }

    public bool OnError(MediaPlayer mp, MediaError what, int extra)
    {
        Logger.LogError($"{nameof(OnError)}: {what} - {extra}");

        UpdatePlaybackState(PlaybackStateCode.Error);
        return true;
    }

    public void OnPrepared(MediaPlayer mp)
    {
        Logger.LogInformation("Media prepared.");
        
        mp.Start();
        UpdatePlaybackState(PlaybackStateCode.Playing);

        UpdateSessionMetaDataAndLoadImage();
    }

    private int _LatestValidPosition = -1;

    public int Position
    {
        get
        {
            var pos = RawPosition;

            if (pos >= 0)
            {
                _LatestValidPosition = pos;
            }

            return _LatestValidPosition;
        }
        private set
        {
            _LatestValidPosition = value;
        }
    }

    private int RawPosition
    {
        get
        {
            // Note: On buffering, position and duration is probably not valid.
            if (mediaPlayer is null ||
                (MediaPlayerState != PlaybackStateCode.Playing && MediaPlayerState != PlaybackStateCode.Paused && MediaPlayerState != PlaybackStateCode.Buffering)
                )
            {
                return -1;
            }
            else
            {
                return mediaPlayer.CurrentPosition;
            }
        }
    }

    private int _LatestValidDuration = -1;

    public int Duration
    {
        get
        {
            var duration = RawDuration;

            if (duration > 0)
            {
                _LatestValidDuration = duration;
            }

            return _LatestValidDuration;
        }
        set
        {
            _LatestValidDuration = value;
        }
    }

    private int RawDuration
    {
        get
        {
            if (mediaPlayer is null
                || (MediaPlayerState != PlaybackStateCode.Playing
                    && MediaPlayerState != PlaybackStateCode.Paused))
            {
                return 0;
            }
            else
            {
                return mediaPlayer.Duration;
            }
        }
    }

    private int buffered = 0;

    public int Buffered
    {
        get
        {
            if (mediaPlayer is null)
            {
                return 0;
            }
            else
            {
                return buffered;
            }
        }
        private set
        {
            buffered = value;
        }
    }

    /// <summary>
    /// Intializes the player.
    /// </summary>
    public Task Play(PlayList playlist = null)
    {
        if (playlist is not null)
        {
            Logger.LogInformation($"{nameof(Play)} with url {0}", playlist.CurrentItem.PreferablePlayUrl);

            if (mediaPlayer is null)
            {
                InitializePlayer();
            }

            if (mediaSession is null)
            {
                InitMediaSession();
            }

            if (PlayList != playlist)
            {
                ItemImage = null;
            }

            PlayList = playlist;

            return PrepareAndPlayMediaPlayerAsync();
        }
        else
        {
            Logger.LogInformation($"{nameof(Play)} with existing source.");

            // Restart if stopped
            if (MediaPlayerState == PlaybackStateCode.Stopped)
            {
                Logger.LogInformation($"Player is stopped, maybe something went wrong. Restarting.");

                if (PlayList is not null)
                {
                    return Play(PlayList);
                }
                else
                {
                    return Task.CompletedTask;
                }
            }

            if (mediaPlayer is null)
            {
                return Task.CompletedTask;
            }

            if (mediaPlayer.IsPlaying)
            {
                UpdatePlaybackState(PlaybackStateCode.Playing);

                return Task.CompletedTask;
            }

            mediaPlayer.Start();
            UpdatePlaybackState(PlaybackStateCode.Playing);

            //Update the metadata now that we are playing
            UpdateSessionMetaData();

            return Task.CompletedTask;
        }
    }

    private async Task PrepareAndPlayMediaPlayerAsync()
    {
        AudioCounter++;
        int audioCounter = AudioCounter;

        var item = PlayList?.CurrentItem;

        if (item is null)
        {
            return;
        }
            
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Logger.LogInformation("Starts playing from {0}", item.PreferablePlayUrl);

                UpdatePlaybackState(PlaybackStateCode.Buffering);

                await Task.Run(async() =>
                {
                    // I have not ide why a pause and then a delay is necessary
                    // before reset. But if not, it will instead throw an 
                    // IllegalStateException when running PrepareAsync.
                    mediaPlayer.Pause();

                    // Make sure item hasn't been changed.
                    if (audioCounter != AudioCounter)
                    {
                        return;
                    }

                    var focusResult = audioManager.RequestAudioFocus(new AudioFocusRequestClass
                        .Builder(AudioFocus.Gain)
                        .SetOnAudioFocusChangeListener(this)
                        .Build());

                    if (focusResult != AudioFocusRequest.Granted)
                    {
                        // Could not get audio focus
                        Logger.LogWarning("Could not get audio focus.");
                    }

                    // Make sure item hasn't been changed.
                    if (audioCounter != AudioCounter)
                    {
                        return;
                    }

                    mediaPlayer.Reset();

                    _LatestValidDuration = -1;
                    _LatestValidPosition = -1;

                    AndroidNet.Uri uri = AndroidNet.Uri.Parse(item.PreferablePlayUrl);
                    await mediaPlayer.SetDataSourceAsync(base.ApplicationContext, uri);

                    // Make sure item hasn't been changed.
                    if (audioCounter != AudioCounter)
                    {
                        return;
                    }

                    mediaPlayer.PrepareAsync();
                });
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"{nameof(PrepareAndPlayMediaPlayerAsync)}: {e.Message}");

            await Stop();
        }
    }

    public async Task Seek(int position)
    {
        Logger.LogInformation($"{nameof(Seek)} - position {position}");
        
        UpdatePlaybackState(PlaybackStateCode.Buffering);
        
        await Task.Run(() => 
        {
            if (mediaPlayer is not null)
            {
                Position = position;
                mediaPlayer.SeekTo(position);
            }
        });
    }

    public void PlayNext()
    {
        Logger.LogInformation("Requesting skipping to next.");

        UpdatePlaybackState(PlaybackStateCode.SkippingToNext);
    }


    public async Task PlayPause()
    {
        Logger.LogInformation($"{nameof(PlayPause)}");
        
        if (mediaPlayer is null || (mediaPlayer is not null && MediaPlayerState == PlaybackStateCode.Paused))
        {
            await Play();
        }
        else
        {
            await Pause();
        }
    }

    public async Task Pause()
    {
        Logger.LogInformation($"{nameof(Pause)}");
        
        await Task.Run(() => {
            if (mediaPlayer is null)
            {
                return;
            }

            if (mediaPlayer.IsPlaying)
            {
                mediaPlayer.Pause();
            }

            UpdatePlaybackState(PlaybackStateCode.Paused);
        });
    }

    public async Task Stop()
    {
        Logger.LogInformation($"{nameof(Stop)}");

        await Task.Run(() => {
            if (mediaPlayer is null)
            {
                return;
            }

            if (mediaPlayer.IsPlaying)
            {
                mediaPlayer.Stop();
            }

            UpdatePlaybackState(PlaybackStateCode.Stopped);
            mediaPlayer.Reset();
            mediaPlayer.Release();
            mediaPlayer = null;
            NotificationHelper.StopNotification(ApplicationContext);
            StopForeground(true);
            ReleaseWifiLock();
            UnregisterMediaSessionCompat();
        });
    }

    private void UpdatePlaybackState(PlaybackStateCode state)
    {
        if (mediaSession is null || mediaPlayer is null)
        {
            return;
        }

        try
        {
            PlaybackState.Builder stateBuilder = new PlaybackState.Builder()
                .SetActions(
                    PlaybackState.ActionPause |
                    PlaybackState.ActionPlay |
                    PlaybackState.ActionPlayPause |
                    PlaybackState.ActionSkipToNext |
                    PlaybackState.ActionSkipToPrevious |
                    PlaybackState.ActionStop |
                    PlaybackState.ActionSeekTo
                )
                .SetState(state, RawPosition, 1.0f);

            mediaSession.SetPlaybackState(stateBuilder.Build());

            OnStatusChanged(EventArgs.Empty);

            UpdateSessionMetaData();

            UpdateWifiLock();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error in {nameof(UpdatePlaybackState)}");
        }
    }

    internal void SetMuted(bool value)
    {
        mediaPlayer.SetVolume(0, 0);
    }

    internal void SetVolume(float value)
    {
        mediaPlayer.SetVolume(value, value);
    }

    private async void UpdateSessionMetaDataAndLoadImage()
    {
        PlayListItem item = PlayList.CurrentItem!;

        UpdateSessionMetaData();

        if (!string.IsNullOrEmpty(item.IconUri))
        {
            Bitmap bitmap = null;

            Logger.LogInformation("Loading image from {0}", item.IconUri);
            
            await Task.Run(async () =>
            {
                try
                {
                    URL url = new URL(item.IconUri);
                    bitmap = await BitmapFactory.DecodeStreamAsync(url.OpenStream());
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Failed to load image.");
                }
            });

            // Make sure not another items has started.
            if (bitmap is not null)
            {
                if (item == PlayList.CurrentItem)
                {
                    Logger.LogInformation("Updating session data with new image.");
                    
                    ItemImage = bitmap;
                    UpdateSessionMetaData();
                }
                else
                {
                    Logger.LogInformation("Will not update session data with new image. Playlist item has changed.");
                }
            }
        }
    }

    private void UpdateNotification()
    {
        if (mediaSession is null)
        {
            return;
        }

        NotificationHelper.StartNotification(
            ApplicationContext,
            mediaController.Metadata,
            mediaSession,
            ItemImage,
            MediaPlayerState == PlaybackStateCode.Playing);
    }

    /// <summary>
    /// Updates the metadata on the lock screen
    /// </summary>
    private void UpdateSessionMetaData()
    {
        if (mediaSession is null)
        {
            return;
        }

        var item = PlayList.CurrentItem!;

        MediaMetadata.Builder builder = new MediaMetadata.Builder();

        builder
            .PutString(MediaMetadata.MetadataKeyArtist, item.Episode ?? string.Empty)
            .PutString(MediaMetadata.MetadataKeyTitle, item.Program ?? item.Channel ?? string.Empty);

        var cover = ItemImage;

        if (cover is not null)
        {
            builder.PutBitmap(MediaMetadata.MetadataKeyAlbumArt, cover);
        }

        // Add duration
        if (!item.IsLiveAudio)
        {
            long duration = Duration;

            builder.PutLong(MediaMetadata.MetadataKeyDuration, Math.Max(0, duration));
        }

        mediaSession.SetMetadata(builder.Build());

        UpdateNotification();
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        HandleIntent(intent);
        return base.OnStartCommand(intent, flags, startId);
    }

    private void HandleIntent(Intent intent)
    {
        if (intent is null || intent.Action is null || mediaController is null)
        {
            return;
        }

        string action = intent.Action;

        if (action.Equals(ActionPlay))
        {
            mediaController.GetTransportControls().Play();
        }
        else if (action.Equals(ActionPause))
        {
            mediaController.GetTransportControls().Pause();
        }
        else if (action.Equals(ActionRewind))
        {
            mediaController.GetTransportControls().Rewind();
        }
        else if (action.Equals(ActionForward))
        {
            mediaController.GetTransportControls().FastForward();
        }
        else if (action.Equals(ActionPrevious))
        {
            mediaController.GetTransportControls().SkipToPrevious();
        }
        else if (action.Equals(ActionNext))
        {
            mediaController.GetTransportControls().SkipToNext();
        }
        else if (action.Equals(ActionStop))
        {
            mediaController.GetTransportControls().Stop();
        }
    }

    private void UpdateWifiLock()
    {
        if (MediaPlayerState is PlaybackStateCode.None or PlaybackStateCode.Stopped or PlaybackStateCode.Error)
        {
            Logger.LogInformation("State is {0}, wifi not needed.", MediaPlayerState);
            ReleaseWifiLock();
        }
        else if (PlayList?.CurrentItem is null || PlayList.RequiresInternet == false)
        {
            Logger.LogInformation("Play list doesn't need Internet, wifi not needed.", MediaPlayerState);
            ReleaseWifiLock();
        }
        else
        {
            Logger.LogInformation("State is {0} and play list requires Internet, will try lock wifi.", MediaPlayerState);
            AquireWifiLock();
        }
    }

    /// <summary>
    /// Lock the wifi so we can still stream under lock screen
    /// </summary>
    private void AquireWifiLock()
    {
        if (wifiLock is not null)
        {
            Logger.LogInformation("Wifi lock already aquired.");
            return;
        }

        Logger.LogInformation("Aquire wifi lock.");
        
        wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "xamarin_wifi_lock");

        wifiLock.Acquire();
    }

    /// <summary>
    /// This will release the wifi lock if it is no longer needed
    /// </summary>
    private void ReleaseWifiLock()
    {
        if (wifiLock is null)
        {
            Logger.LogInformation("No wifi lock aquired.");
            return;
        }

        Logger.LogInformation("Releasing wifi lock.");
        
        wifiLock.Release();
        wifiLock = null;
    }

    private void UnregisterMediaSessionCompat()
    {
        try
        {
            if (mediaSession is not null)
            {
                mediaSession.Dispose();
                mediaSession = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error in {nameof(UnregisterMediaSessionCompat)}");
        }
    }

    IBinder binder;

    public override IBinder OnBind(Intent intent)
    {
        binder = new MediaPlayerServiceBinder(this);
        return binder;
    }

    public override bool OnUnbind(Intent intent)
    {
        NotificationHelper.StopNotification(ApplicationContext);
        return base.OnUnbind(intent);
    }

    /// <summary>
    /// Properly cleanup of your player by releasing resources
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (mediaPlayer is not null)
        {
            mediaPlayer.Release();
            mediaPlayer = null;

            NotificationHelper.StopNotification(ApplicationContext);
            StopForeground(true);
            ReleaseWifiLock();
            UnregisterMediaSessionCompat();
        }
    }

    private bool RestartAudioOnGainAudioFocus = false;

    public async void OnAudioFocusChange(AudioFocus focusChange)
    {
        Logger.LogInformation($"{nameof(OnAudioFocusChange)} - {focusChange}");

        switch (focusChange)
        {
            case AudioFocus.Gain:
                Logger.LogInformation("Gaining audio focus.");

                if (RestartAudioOnGainAudioFocus)
                {
                    Logger.LogInformation("Restarting audio.");

                    _ = Play();
                }
                else
                {
                    Logger.LogInformation("Restarting audio not needed.");
                }

                break;
            case AudioFocus.Loss:
                Logger.LogInformation("Permanent lost audio focus.");
                RestartAudioOnGainAudioFocus = false;

                //We have lost focus stop!
                await Stop();
                break;
            case AudioFocus.LossTransient:
                Logger.LogInformation("Transient lost audio focus.");
                
                //We have lost focus for a short time

                // Restart if playing
                if (this.MediaPlayerState == PlaybackStateCode.Playing)
                {
                    Logger.LogInformation("Was playing. Will restart audio on gain audio focus.");
                    RestartAudioOnGainAudioFocus = true;
                }
                else
                {
                    Logger.LogInformation("Was not playing. Will not restart audio on gain audio focus.");
                    RestartAudioOnGainAudioFocus = false;
                }

                await Pause();
                break;

            case AudioFocus.LossTransientCanDuck:
                //We have lost focus but should till play at a muted 10% volume
                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.SetVolume(.1f, .1f);
                }

                break;
        }
    }

    public class MediaSessionCallback : MediaSession.Callback
    {
        private readonly MediaPlayerServiceBinder mediaPlayerService;
        public MediaSessionCallback(MediaPlayerServiceBinder service)
        {
            mediaPlayerService = service;
        }

        public override void OnPause()
        {
            WeakReferenceMessenger.Default.Send(new ExternalMediaButtonPressed(ExternalMediaButton.Pause));
            base.OnPause();
        }

        public override void OnPlay()
        {
            WeakReferenceMessenger.Default.Send(new ExternalMediaButtonPressed(ExternalMediaButton.Play));
            base.OnPlay();
        }

        public override void OnRewind()
        {
            WeakReferenceMessenger.Default.Send(new ExternalMediaButtonPressed(ExternalMediaButton.Rewind));
            base.OnRewind();
        }

        public override void OnFastForward()
        {
            WeakReferenceMessenger.Default.Send(new ExternalMediaButtonPressed(ExternalMediaButton.Forward));
            base.OnFastForward();
        }
        
        public override void OnSkipToPrevious()
        {
            WeakReferenceMessenger.Default.Send(new ExternalMediaButtonPressed(ExternalMediaButton.Previous));
            base.OnSkipToPrevious();
        }

        public override void OnSkipToNext()
        {
            WeakReferenceMessenger.Default.Send(new ExternalMediaButtonPressed(ExternalMediaButton.Next));
            base.OnSkipToNext();
        }


        public override async void OnStop()
        {
            await mediaPlayerService.GetMediaPlayerService().Stop();
            base.OnStop();
        }

        public override void OnSeekTo(long pos)
        {
            _ = mediaPlayerService.GetMediaPlayerService().Seek((int)pos);
            base.OnSeekTo(pos);
        }
    }
}

public class MediaPlayerServiceBinder : Binder
{
    private readonly MediaPlayerService service;

    public MediaPlayerServiceBinder(MediaPlayerService service)
    {
        this.service = service;
    }

    public MediaPlayerService GetMediaPlayerService()
    {
        return service;
    }
}
