using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Java.Net;
using Microsoft.NetConf2021.Maui.Platforms.Android.Receivers;
using Pekspro.RadioStorm.MAUI;
using AndroidNet = Android.Net;

#nullable disable

namespace Microsoft.NetConf2021.Maui.Platforms.Android.Services;

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

    public event PlayingEventHandler Playing;

    public event BufferingEventHandler Buffering;

    public string AudioUrl { get; private set; }

    public PlayListItem Item;

    private readonly Handler PlayingHandler;
    private readonly Java.Lang.Runnable PlayingHandlerRunnable;

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
        PlayingHandler = new Handler(Looper.MainLooper);

        // Create a runnable, restarting itself if the status still is "playing"
        PlayingHandlerRunnable = new Java.Lang.Runnable(() => 
        {
            OnPlaying(EventArgs.Empty);

            if (MediaPlayerState == PlaybackStateCode.Playing)
            {
                PlayingHandler.PostDelayed(PlayingHandlerRunnable, 250);
            }
        });

        // On Status changed to PLAYING, start raising the Playing event
        StatusChanged += (sender, e) => 
        {
            var state = MediaPlayerState;

            Logger.LogInformation($"StatusChanged: {state}");

            if (state == PlaybackStateCode.Playing)
            {
                PlayingHandler.PostDelayed(PlayingHandlerRunnable, 0);
            }
        };
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

    protected virtual void OnPlaying(EventArgs e)
    {
        Playing?.Invoke(this, e);
    }

    protected virtual void OnBuffering(EventArgs e)
    {
        Buffering?.Invoke(this, e);
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
        int duration = 0;
        if (MediaPlayerState == PlaybackStateCode.Playing || MediaPlayerState == PlaybackStateCode.Paused)
        {
            duration = mp.Duration;
        }

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
        mp.Start();
        UpdatePlaybackState(PlaybackStateCode.Playing);
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
            OnBuffering(EventArgs.Empty);
        }
    }

    private Bitmap cover;

    private Bitmap Cover
    {
        get
        {
            return cover;
        }
        set
        {
            cover = value;

            if (cover is not null)
            {
                StartNotification();
                UpdateMediaMetadataCompat();
            }
        }
    }

    public Task Play(string audioUrl)
    {
        AudioUrl = audioUrl;
        _LatestValidDuration = -1;
        _LatestValidPosition = -1;

        return Play();
    }

    /// <summary>
    /// Intializes the player.
    /// </summary>
    public async Task Play()
    {
        Logger.LogInformation($"{nameof(Play)}");
      
        // Has state SkippingToNext when playlist is completed.
        if (mediaPlayer is not null && MediaPlayerState is PlaybackStateCode.Paused or PlaybackStateCode.SkippingToNext)
        {
            //We are simply paused so just start again
            mediaPlayer.Start();
            UpdatePlaybackState(PlaybackStateCode.Playing);
            StartNotification();

            //Update the metadata now that we are playing
            UpdateMediaMetadataCompat();
            return;
        }

        if (mediaPlayer is null)
        {
            InitializePlayer();
        }

        if (mediaSession is null)
        {
            InitMediaSession();
        }

        if (mediaPlayer.IsPlaying)
        {
            UpdatePlaybackState(PlaybackStateCode.Playing);
            return;
        }

        await PrepareAndPlayMediaPlayerAsync();
    }

    private async Task PrepareAndPlayMediaPlayerAsync()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {

                AndroidNet.Uri uri = AndroidNet.Uri.Parse(AudioUrl);
                await mediaPlayer.SetDataSourceAsync(base.ApplicationContext, uri);
                
                var focusResult = audioManager.RequestAudioFocus(new AudioFocusRequestClass
                    .Builder(AudioFocus.Gain)
                    .SetOnAudioFocusChangeListener(this)
                    .Build());

                if (focusResult != AudioFocusRequest.Granted)
                {
                    // Could not get audio focus
                    Logger.LogWarning("Could not get audio focus.");
                }

                UpdatePlaybackState(PlaybackStateCode.Buffering);
                mediaPlayer.PrepareAsync();

                AquireWifiLock();
                UpdateMediaMetadataCompat();
                StartNotification();

                try
                {
                    Bitmap bitmap = null;

                    await Task.Run(async () =>
                    {
                        try
                        {
                            URL url = new URL(Item.IconUri);
                            bitmap = await BitmapFactory.DecodeStreamAsync(url.OpenStream());
                        }
                        catch (Exception )
                        {
                                
                        }
                    });
                        
                    Cover = bitmap;
                }
                catch (Exception )
                {
                    Cover = null;
                }
            }
        }
        catch (Exception)
        {
            UpdatePlaybackStateStopped();
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

    private async Task<bool> TrySeek(TimeSpan length)
    {
        var position = Position;
        var duration = Duration;

        if (position < 0 || duration <= 0)
        {
            return false;
        }

        int newPosition = position + (int) length.TotalMilliseconds;

        if (newPosition < 0)
        {
            newPosition = 0;
        }

        if (newPosition > duration)
        {
            return false;
        }

        await Seek(newPosition);

        return true;
    }

    public void PlayNext()
    {
        if (mediaPlayer is not null)
        {
            mediaPlayer.Pause();
        }

        UpdatePlaybackState(PlaybackStateCode.SkippingToNext);
    }

    public void PlayPrevious()
    {
        if (mediaPlayer is not null)
        {
            mediaPlayer.Pause();
        }

        UpdatePlaybackState(PlaybackStateCode.SkippingToPrevious);
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
            NotificationHelper.StopNotification(ApplicationContext);
            StopForeground(true);
            ReleaseWifiLock();
            UnregisterMediaSessionCompat();
        });
    }

    public void UpdatePlaybackStateStopped()
    {
        UpdatePlaybackState(PlaybackStateCode.Stopped);

        if (mediaPlayer != null)
        {
            mediaPlayer.Reset();
            mediaPlayer.Release();
            mediaPlayer = null;
        }
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

            if (state == PlaybackStateCode.Playing)
            {
                // Durtion may not be set in notificiation bar, this will set it.
                UpdateMediaMetadataCompat();
            }

            StartNotification();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error in {nameof(UpdateMediaMetadataCompat)}");
        }
    }

    private void StartNotification()
    {
        if (mediaSession is null)
        {
            return;
        }

        NotificationHelper.StartNotification(
            ApplicationContext,
            mediaController.Metadata,
            mediaSession,
            Cover,
            MediaPlayerState == PlaybackStateCode.Playing);
    }

    internal void SetMuted(bool value)
    {
        mediaPlayer.SetVolume(0, 0);
    }

    internal void SetVolume(float value)
    {
        mediaPlayer.SetVolume(value, value);
    }

    /// <summary>
    /// Updates the metadata on the lock screen
    /// </summary>
    private void UpdateMediaMetadataCompat()
    {
        if (mediaSession is null)
        {
            return;
        }

        MediaMetadata.Builder builder = new MediaMetadata.Builder();

        builder
            // .PutString(MediaMetadata.MetadataKeyAlbum, "")
            .PutString(MediaMetadata.MetadataKeyArtist, Item.Episode ?? string.Empty)
            .PutString(MediaMetadata.MetadataKeyTitle, Item.Program ?? Item.Channel ?? string.Empty);

        var cover = Cover;

        if (cover is not null)
        {
            builder.PutBitmap(MediaMetadata.MetadataKeyAlbumArt, cover);
        }

        // Add duration
        if (!Item.IsLiveAudio)
        {
            long duration = mediaPlayer?.Duration ?? 0;

            builder.PutLong(MediaMetadata.MetadataKeyDuration, Math.Max(0, duration));
        }

        mediaSession.SetMetadata(builder.Build());
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

    /// <summary>
    /// Lock the wifi so we can still stream under lock screen
    /// </summary>
    private void AquireWifiLock()
    {
        if (wifiLock is null)
        {
            wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "xamarin_wifi_lock");
        }
        wifiLock.Acquire();
    }

    /// <summary>
    /// This will release the wifi lock if it is no longer needed
    /// </summary>
    private void ReleaseWifiLock()
    {
        if (wifiLock is null)
        {
            return;
        }

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
