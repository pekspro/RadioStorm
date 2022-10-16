namespace Pekspro.RadioStorm.Audio;

public abstract class AudioManagerBase : IAudioManager
{
    #region Private properties

    private double[] PlaybackRates = new double[] { 0.5, 0.75, 1, 1.5, 2 };

    private const int DefaultPlaybackRate = 2;

    private readonly TimeSpan SleepVolumeFadeStartTime = TimeSpan.FromSeconds(60);

    private IRecentPlayedManager RecentPlayedManager { get; }

    private IDownloadManager DownloadManager { get; }

    private ILocalSettings LocalSettings { get; }

    private System.Timers.Timer? RefreshPositionTimer;

    private IMainThreadTimer? MainThreadRefreshPositionTimer;

    private IListenStateManager ListenStateManager { get; }

    private IMessenger Messenger { get; }

    private IDateTimeProvider DateTimeProvider { get; }

    protected ILogger Logger { get; }

    private enum RestorePostionMode { None, Restore, RestoreAtAnyMargin };

    private RestorePostionMode RestorePosition;

    private int LatestOnLengthAndPositionUpdatedItemAudioId = -1;

    private bool LatestOnLengthAndPositionUpdatedTriedSetToTrue = false;

    private SeekSizeProvider SeekSizeProvider = new SeekSizeProvider();

    public static readonly TimeSpan DefaultSleepTimerDelta = TimeSpan.FromMinutes(2);

    #endregion

    #region Abstract

    protected abstract void MediaPlay(PlayList playlist);
    protected abstract void MediaPlay();
    protected abstract void MediaPause();
    protected abstract void MediaStop();
    protected abstract void MediaSetPlaybackPosition(TimeSpan position);
    protected abstract void MediaRefreshButtonStates();
    protected abstract void MediaRefreshLengthAndPosition();
    abstract protected void MediaSetVolume(double volume);
    abstract protected void SetPlaybackRate(double speedRatio);

    #endregion

    #region Constructor

    public AudioManagerBase(
            IMainThreadTimerFactory mainThreadTimerFactory,
            IMainThreadRunner mainThreadRunner,
            IListenStateManager listenStateManager,
            IRecentPlayedManager recentPlayedManager,
            IDownloadManager downloadManager,
            ILocalSettings localSettings,
            IMessenger messenger,
            IDateTimeProvider dateTimeProvider,
            ILogger logger,
            bool useMainThreadTimer
        )
    {
        RecentPlayedManager = recentPlayedManager;
        DownloadManager = downloadManager;
        LocalSettings = localSettings;
        ListenStateManager = listenStateManager;
        Messenger = messenger;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;

        Volume = localSettings.Volume;

        if (useMainThreadTimer)
        {
            MainThreadRefreshPositionTimer = mainThreadTimerFactory.CreateTimer("Audio refresh");
            MainThreadRefreshPositionTimer.Interval = 1000;
            MainThreadRefreshPositionTimer.SetupCallBack(OnTimerTick);
        }
        else
        {
            RefreshPositionTimer = new();
            RefreshPositionTimer.Interval = 1000;
            RefreshPositionTimer.Elapsed += (a, b) => { OnTimerTick(); };
        }

        messenger?.Register<DownloadUpdated>(this, (r, m) =>
        {
            if (m.Download.Status != DownloadDataStatus.Done)
            {
                return;
            }

            if (useMainThreadTimer)
            {
                mainThreadRunner.RunInMainThread(() =>
                {
                    OnDownloadUpdated(m);
                });
            }
            else
            {
                OnDownloadUpdated(m);
            }
        });

        messenger?.Register<ExternalMediaButtonPressed>(this, (r, m) =>
        {
            switch (m.Button)
            {
                case ExternalMediaButton.Play:
                    Play();
                    break;

                case ExternalMediaButton.Pause:
                    Pause();
                    break;

                case ExternalMediaButton.PlayPause:
                    PlayPause();
                    break;

                case ExternalMediaButton.Rewind:
                    Move(SeekSizeProvider.RewindSize);
                    SeekSizeProvider.Decrease();
                    break;

                case ExternalMediaButton.Forward:
                    Move(SeekSizeProvider.ForwardSize);
                    SeekSizeProvider.Increase();
                    break;

                case ExternalMediaButton.Next:
                    GoToNext();
                    break;

                case ExternalMediaButton.Previous:
                    GoToPrevious();
                    break;

                default:
                    break;
            }
        });
    }

    #endregion

    #region Properties

    public bool CanPlay { get; set; }

    #region CanPause property

    private bool _CanPause;

    public bool CanPause
    {
        get
        {
            return _CanPause;
        }
        set
        {
            _CanPause = value;

            if (MainThreadRefreshPositionTimer is not null)
            {
                if (MainThreadRefreshPositionTimer.Enabled != value)
                {
                    MainThreadRefreshPositionTimer.Enabled = value;

                    Logger.LogInformation($"Set {nameof(MainThreadRefreshPositionTimer)} enabled to {value}.");
                }
            }

            if (RefreshPositionTimer is not null)
            {
                if (RefreshPositionTimer.Enabled != value)
                {
                    RefreshPositionTimer.Enabled = value;

                    Logger.LogInformation($"Set {nameof(RefreshPositionTimer)} enabled to {value}.");
                }
            }
        }
    }

    #endregion

    public bool CanPlayPause => CanPause || CanPlay;

    public bool CanStop => CurrentPlayList is not null;

    public bool IsBuffering { get; set; }

    public TimeSpan Position { get; set; }

    public TimeSpan MediaLength { get; set; }

    public int ProgressMaxValue { get; set; }

    public int ProgressValue { get; set; }

    public double PlaybackRate => PlaybackRates[_PlaybackRateIndex];


    private int _PlaybackRateIndex = DefaultPlaybackRate;

    public int PlaybackRateIndex
    {
        get
        {
            return _PlaybackRateIndex;
        }
        set
        {
            if (value >= 0 && value < PlaybackRates.Length && value != _PlaybackRateIndex)
            {
                // Only default speed can be used on live audio.
                if (value == DefaultPlaybackRate || CurrentPlayList?.IsLiveAudio != true)
                {
                    _PlaybackRateIndex = value;

                    Logger.LogInformation("Setting play back index {index} ({playbackRate})", value, PlaybackRate);

                    int timerIntervall;
                    if (PlaybackRateIndex > DefaultPlaybackRate)
                    {
                        // Refresh more often if faster than default.
                        timerIntervall = 1000 / 4;
                    }
                    else
                    {
                        timerIntervall = 1000;
                    }

                    if (MainThreadRefreshPositionTimer is not null)
                    {
                        MainThreadRefreshPositionTimer.Interval = timerIntervall;
                    }

                    if (RefreshPositionTimer is not null)
                    {
                        RefreshPositionTimer.Interval = timerIntervall;
                    }

                    SetPlaybackRate(PlaybackRate);
                }

                Messenger.Send(new SpeedRateChanged(PlaybackRateIndex, PlaybackRate));
            }
        }
    }

    private double? _BufferRatio;

    public double? BufferRatio
    {
        get
        {
            return _BufferRatio;
        }
        set
        {
            if (value != _BufferRatio)
            {
                Logger.LogInformation("New buffer ratio {bufferRatio:0.000}", value);

                _BufferRatio = value;

                Messenger.Send(new BufferRatioChanged(value));
            }
        }
    }

    #region Volume property

    private double _Volume;

    public double Volume
    {
        get
        {
            return _Volume;
        }
        set
        {
            _Volume = value;
            LocalSettings.Volume = _Volume;
            UpdateVolume();
        }
    }

    private void UpdateVolume()
    {
        MediaSetVolume(_Volume * SleepModeVolumeMultiplier);
    }

    #endregion

    #region Sleep state properties

    public bool IsSleepTimerEnabled { get; private set; }

    public DateTime SleepActivationTime { get; private set; }

    public TimeSpan TimeLeftToSleepActivation
    {
        get
        {
            if (IsSleepTimerEnabled)
            {
                var timeLeft = SleepActivationTime - DateTimeProvider.UtcNow;

                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = TimeSpan.Zero;
                }

                return timeLeft;
            }
            else
            {
                return TimeSpan.Zero;
            }
        }
    }

    public double SleepModeVolumeMultiplier
    {
        get
        {
            if (!IsSleepTimerEnabled)
            {
                return 1;
            }

            var timeLeft = TimeLeftToSleepActivation;

            if (timeLeft > SleepVolumeFadeStartTime)
            {
                return 1;
            }

            return timeLeft.TotalMilliseconds / SleepVolumeFadeStartTime.TotalMilliseconds;
        }
    }

    #endregion

    public PlayListItem? CurrentItem => CurrentPlayList?.CurrentItem;

    public PlayList? CurrentPlayList { get; set; }

    #endregion

    #region Methods

    protected void RestorePositionOnNextPlay()
    {
        RestorePosition = RestorePostionMode.RestoreAtAnyMargin;
    }

    public void Play()
    {
        // File just downloaded?
        if (TryUpdateCurrentItemDownloadedFile())
        {
            // Switch url
            Logger.LogInformation($"Switches to audio url/path: {CurrentItem!.PreferablePlayUrl}");
            BufferRatio = null;
            MediaPlay(CurrentPlayList!);
            RestorePosition = RestorePostionMode.RestoreAtAnyMargin;
        }
        else
        {
            Logger.LogInformation($"{nameof(Play)} on position: {{position}} (length {{length}})", Position, MediaLength);

            MediaPlay();
        }
    }

    public void Pause()
    {
        if (CanPause)
        {
            Logger.LogInformation($"{nameof(Pause)} on position: {{position}} (length {{length}})", Position, MediaLength);

            MediaPause();
        }
    }

    public void PlayPause()
    {
        Logger.LogInformation($"{nameof(PlayPause)}");

        if (CanPause)
        {
            Pause();
        }
        else if (CanPlay)
        {
            Play();
        }
    }

    public void Stop()
    {
        Logger.LogInformation($"{nameof(Stop)} on position: {{position}} (length {{length}})", Position, MediaLength);

        MediaStop();

        CurrentPlayList = null;
        SendPlaylistChanged();
        UpdateButtonStates(false, false, false);
    }

    public void Move(TimeSpan delta)
    {
        Logger.LogInformation($"{nameof(Move)} delta: {delta}");

        if (TrySeek(delta))
        {
            return;
        }

        if (delta < TimeSpan.Zero)
        {
            GoToPrevious();
        }
        else
        {
            GoToNext();
        }
    }

    private bool TrySeek(TimeSpan length)
    {
        var position = Position;
        var duration = MediaLength;

        if (position < TimeSpan.Zero || duration < TimeSpan.Zero)
        {
            return false;
        }

        var newPosition = position + length;

        if (newPosition > duration)
        {
            // Less than 5 seconds from the end?
            if (position.Add(TimeSpan.FromSeconds(5)) >= duration)
            {
                // Don't do anything
                return false;
            }

            // Move to 5 seconds from the send
            newPosition = duration.Add(TimeSpan.FromSeconds(-5));
        }

        if (newPosition < TimeSpan.Zero)
        {
            newPosition = TimeSpan.Zero;
        }

        SetPlaybackPosition(newPosition);

        return true;
    }

    public bool GoToNext()
    {
        if (CurrentPlayList is null)
        {
            Logger.LogWarning("No playlist, cannot go to next.");
            return false;
        }

        if (CurrentPlayList.CurrentPosition + 1 >= CurrentPlayList.Items.Count)
        {
            Logger.LogWarning("This is last item on list. Cannot go to next.");
            return false;
        }

        Logger.LogInformation("Go to next playlist item.");

        CurrentPlayList.CurrentPosition++;
        SendCurrentItemChanged();

        SetPlaylistPosition(CurrentPlayList.CurrentPosition);

        return true;
    }

    public bool GoToPrevious()
    {
        if (CurrentPlayList is null)
        {
            Logger.LogWarning("No playlist, cannot go to previous.");
            return false;
        }

        if (Position < TimeSpan.FromSeconds(5))
        {
            Logger.LogInformation("Moves to start instead of go to previous.");
            SetPlaybackPosition(TimeSpan.Zero);
        }

        if (CurrentPlayList.CurrentPosition <= 0)
        {
            Logger.LogWarning("This is first item on list. Cannot go to previous.");
            return false;
        }

        Logger.LogInformation("Moves to previous playlist item.");

        CurrentPlayList.CurrentPosition--;
        SendCurrentItemChanged();

        SetPlaylistPosition(CurrentPlayList.CurrentPosition);

        return true;
    }

    private bool TryUpdateCurrentItemDownloadedFile()
    {
        if (CurrentPlayList is null)
        {
            return false;
        }

        string? localUrl = null;
        var currentItem = CurrentPlayList.CurrentItem!;

        if (currentItem.IsLiveAudio == false)
        {
            var downloadData = DownloadManager.GetDownloadData(currentItem.ProgramId, currentItem.AudioId);

            if (downloadData?.Status == DownloadDataStatus.Done)
            {
                localUrl = downloadData.Filename;
            }
        }

        if (currentItem.LocalUrl != localUrl)
        {
            currentItem.LocalUrl = localUrl;
            return true;
        }

        return false;
    }

    public void SetPlaylistPosition(int pos)
    {
        if (CurrentPlayList is null)
        {
            return;
        }

        RestorePosition = RestorePostionMode.Restore;
        CurrentPlayList.CurrentPosition = pos;
        TryUpdateCurrentItemDownloadedFile();

        Logger.LogInformation($"Will play audio url/path: {CurrentItem!.PreferablePlayUrl}");
        BufferRatio = null;
        MediaPlay(CurrentPlayList);

        RecentPlayedManager.AddOrUpdate(!CurrentItem!.IsLiveAudio, CurrentItem.AudioId);

        SendCurrentItemChanged();
    }

    public void SetPlaybackPosition(TimeSpan position)
    {
        MediaSetPlaybackPosition(position);

        RefreshState();
    }

    private void OnDownloadUpdated(DownloadUpdated m)
    {
        if (CanPause && m.Download.Status == DownloadDataStatus.Done)
        {
            var currentItem = CurrentItem;

            if (currentItem is not null && m.Download.EpisodeId == currentItem.AudioId && m.Download.ProgramId == currentItem.ProgramId)
            {
                // The downloaded item is the one we playing right now. Use Play to switch to local file
                Play();
            }
        }
    }

    private void OnTimerTick()
    {
        Logger.LogDebug($"Ticking position time.");
        MediaRefreshLengthAndPosition();
    }

    public virtual void Play(PlayListItem item)
    {
        Play(new PlayListItem[] { item });
    }

    public void Play(PlayListItem[] items)
    {
        Play(items, Guid.NewGuid(), 0);
    }

    public void Play(PlayListItem[] items, Guid playListId, int activeIndex)
    {
        CurrentPlayList = new PlayList()
        {
            PlayListId = playListId
        };

        foreach (var item in items)
        {
            CurrentPlayList.Items.Add(item);
        }

        if (CurrentPlayList.IsLiveAudio)
        {
            // Reset speed ratio
            PlaybackRateIndex = DefaultPlaybackRate;
        }

        SetPlaylistPosition(0);

        SendPlaylistChanged();
        RefreshState();
    }

    protected void RefreshState()
    {
        MediaRefreshButtonStates();
        MediaRefreshLengthAndPosition();
    }

    public void Add(PlayListItem item)
    {
        Add(new PlayListItem[] { item });
    }

    public void Add(PlayListItem[] items)
    {
        if (CurrentPlayList is null || CurrentPlayList.IsLiveAudio)
        {
            Play(items);
            return;
        }

        var itemsToAdd = items.Where(i => !CurrentPlayList.Items.Any(current => current.AudioId == i.AudioId && current.IsLiveAudio == i.IsLiveAudio)).ToList();

        CurrentPlayList.Items.AddRange(itemsToAdd);

        SendPlaylistChanged();

        OnPlayListChanged();
    }

    private void SendPlaylistChanged(bool itemsMoved = false)
    {
        Messenger.Send(new PlaylistChanged(CurrentPlayList, itemsMoved));
    }

    protected virtual void OnPlayListChanged()
    {

    }

    private void SendCurrentItemChanged()
    {
        if (CurrentPlayList is not null)
        {
            Messenger.Send(new CurrentItemChanged(CurrentPlayList));
        }
    }

    public void RemoveItemFromPlaylist(int itemId)
    {
        RemoveItemsFromPlaylist(new int[] { itemId });
    }

    public void RemoveItemsFromPlaylist(IEnumerable<int> itemIds)
    {
        if (CurrentPlayList is null)
        {
            return;
        }

        bool changed = false;
        bool activeItemChanged = false;

        foreach (var itemId in itemIds)
        {
            if (CurrentPlayList.Items.Count <= 1)
            {
                break;
            }

            int pos = CurrentPlayList.Items.FindIndex(a => a.AudioId == itemId);

            if (pos < 0)
            {
                continue;
            }

            changed = true;
            CurrentPlayList.Items.RemoveAt(pos);

            if (pos == CurrentPlayList.CurrentPosition)
            {
                activeItemChanged = true;
            }
            else if (pos < CurrentPlayList.CurrentPosition)
            {
                CurrentPlayList.CurrentPosition--;
            }
        }

        if (activeItemChanged)
        {
            if (CurrentPlayList.Items.Any())
            {
                SetPlaylistPosition(CurrentPlayList.CurrentPosition);
            }
            else
            {
                Pause();
            }
        }

        if (changed)
        {
            SendPlaylistChanged();
        }
    }

    public void MovePlaylistItem(int fromIndexId, int toIndexId)
    {
        if (CurrentPlayList is null)
        {
            return;
        }

        if (fromIndexId < 0 || fromIndexId >= CurrentPlayList.Items.Count)
        {
            return;
        }

        if (toIndexId < 0 || toIndexId >= CurrentPlayList.Items.Count)
        {
            return;
        }

        if (fromIndexId == toIndexId)
        {
            return;
        }

        var current = CurrentPlayList.Items[fromIndexId];
        CurrentPlayList.Items.RemoveAt(fromIndexId);
        CurrentPlayList.Items.Insert(toIndexId, current);

        if (fromIndexId == CurrentPlayList.CurrentPosition)
        {
            CurrentPlayList.CurrentPosition = toIndexId;
        }
        else
        {
            int currentPosDelta = 0;

            if (fromIndexId < CurrentPlayList.CurrentPosition)
            {
                currentPosDelta--;
            }

            if (toIndexId < CurrentPlayList.CurrentPosition)
            {
                currentPosDelta++;
            }

            CurrentPlayList.CurrentPosition += currentPosDelta;
        }

        OnItemMovedInPlayList(fromIndexId, toIndexId);

        SendPlaylistChanged(true);
    }

    protected virtual void OnItemMovedInPlayList(int fromIndexId, int toIndexId)
    {

    }

    public bool IsItemIdActive(bool isLiveAudio, int itemId)
    {
        if (CurrentItem is null)
        {
            return false;
        }

        if (CurrentItem.IsLiveAudio != isLiveAudio)
        {
            return false;
        }

        return CurrentItem.AudioId == itemId;
    }

    public bool IsItemIdInPlayList(bool isLiveAudio, int itemId)
    {
        if (CurrentPlayList is null)
        {
            return false;
        }

        return CurrentPlayList.Items.Any(i => i.IsLiveAudio == isLiveAudio && i.AudioId == itemId);
    }

    protected void UpdateButtonStates(bool? canPlay, bool? canPause, bool? isBuffering)
    {
        bool changed = false;

        if (canPlay is not null && canPlay.Value != CanPlay)
        {
            CanPlay = canPlay.Value;
            changed = true;
        }

        if (canPause is not null && canPause.Value != CanPause)
        {
            CanPause = canPause.Value;
            changed = true;
        }

        if (isBuffering is not null && isBuffering.Value != IsBuffering)
        {
            IsBuffering = isBuffering.Value;
            changed = true;
        }

        if (changed)
        {
            Messenger.Send(new PlayerButtonStateChanged(CanPlay, CanPause, CanStop, IsBuffering));
        }
    }

    protected void SetLiveAudioPositionAndLength()
    {
        SetPositionAndLength(TimeSpan.Zero, TimeSpan.Zero);
    }

    protected void SetPositionAndLength(TimeSpan position, TimeSpan length)
    {
        Position = position;
        MediaLength = length;
        ProgressMaxValue = (int)MediaLength.TotalSeconds;
        ProgressValue = (int)Position.TotalSeconds;

        Messenger.Send(new MediaPositionChanged(Position, MediaLength));

        OnLengthAndPositionUpdated();
    }

    private void OnLengthAndPositionUpdated()
    {
        if (CurrentItem is not null && CurrentItem.IsLiveAudio == false && ProgressMaxValue > 0)
        {
            bool shouldSetToFullyListen = false;

            if (ProgressMaxValue > 60 && ProgressMaxValue - ProgressValue < 60)
            {
                shouldSetToFullyListen = true;
            }
            else if (ProgressMaxValue <= 60 && ProgressMaxValue - ProgressValue < 1)
            {
                shouldSetToFullyListen = true;
            }

            if (shouldSetToFullyListen &&
                (
                    LatestOnLengthAndPositionUpdatedItemAudioId != CurrentItem.AudioId
                    ||
                    LatestOnLengthAndPositionUpdatedTriedSetToTrue != shouldSetToFullyListen
                )
                )
            {
                ListenStateManager.SetFullyListen(CurrentItem.AudioId, true);
            }

            LatestOnLengthAndPositionUpdatedTriedSetToTrue = shouldSetToFullyListen;
            LatestOnLengthAndPositionUpdatedItemAudioId = CurrentItem.AudioId;


            if (ProgressValue >= 5)
            {
                ListenStateManager.SetListenLength(CurrentItem.AudioId, ProgressValue);
                Logger.LogDebug($"Listen length {ProgressValue}. Audio id: {CurrentItem.AudioId}");
            }
        }
        else
        {
            LatestOnLengthAndPositionUpdatedItemAudioId = -1;
            LatestOnLengthAndPositionUpdatedTriedSetToTrue = false;
        }
    }

    protected void TryRestorePosition(TimeSpan currentMediaDuration)
    {
        if (RestorePosition == RestorePostionMode.None)
        {
            Logger.LogDebug("Restore position disabled.");
            return;
        }

        if (CurrentItem is null || CurrentPlayList is null)
        {
            return;
        }

        if (CurrentItem.IsLiveAudio)
        {
            return;
        }

        var restorePosition = RestorePosition;
        RestorePosition = RestorePostionMode.None;

        int margin = restorePosition == RestorePostionMode.RestoreAtAnyMargin ? 0 : 30;
        int nextPosition = ListenStateManager.GetListenLength(CurrentItem.AudioId);

        Logger.LogInformation($"Should continue from position {nextPosition} (from listen state), margin {margin}.");

        if (nextPosition >= margin && currentMediaDuration.TotalSeconds - margin > nextPosition)
        {
            TimeSpan ts = TimeSpan.FromSeconds(nextPosition);

            Logger.LogInformation($"Setting start position to {ts}.");
            SetPlaybackPosition(ts);
        }
        else
        {
            Logger.LogInformation($"Will NOT set start position to {nextPosition}.");
        }
    }

    private int SleepTimerSessionId = 0;
    
    private void SaveSleepTimerSettings()
    {
        if (IsSleepTimerEnabled)
        {
            LocalSettings.LastSetSleepTimerIntervall = (int) TimeLeftToSleepActivation.TotalSeconds;
        }
    }

    public void StartSleepTimer()
    {
        int sleepTimeInSeconds = Math.Max(60, LocalSettings.LastSetSleepTimerIntervall);

        // Make sure multiple of 60.
        sleepTimeInSeconds = (int)Math.Ceiling(sleepTimeInSeconds / 60.0) * 60;

        StartSleepTimer(TimeSpan.FromSeconds(sleepTimeInSeconds));
    }
    
    public async void StartSleepTimer(TimeSpan timeLeftToSleepActivation)
    {
        SleepTimerSessionId++;

        int sleepModeSessionId = SleepTimerSessionId;

        IsSleepTimerEnabled = true;
        SleepActivationTime = DateTimeProvider.UtcNow.Add(timeLeftToSleepActivation);
        SaveSleepTimerSettings();

        Logger.LogInformation("Sleep mode activated. Sleep time: {sleepTime} ({sleepActivationTime})", timeLeftToSleepActivation, SleepActivationTime);

        while (sleepModeSessionId == SleepTimerSessionId && IsSleepTimerEnabled)
        {
            SendSleepModeMessage();

            var currentTimeLeftToSleepActivation = TimeLeftToSleepActivation;

            if (currentTimeLeftToSleepActivation <= TimeSpan.Zero)
            {
                Logger.LogInformation("Sleep mode activated. Pausing.");

                Pause();
                IsSleepTimerEnabled = false;
                SendSleepModeMessage();

                // Wait one second before restoring volume. This make sure playing has fully stopped before volume is raised again.
                await Task.Delay(1000);
                if (sleepModeSessionId == SleepTimerSessionId)
                {
                    Logger.LogInformation("Restoring volume.");
                    UpdateVolume();
                }

                break;
            }

            if (currentTimeLeftToSleepActivation <= SleepVolumeFadeStartTime)
            {
                Logger.LogInformation("Will sleep soon. Volume multiplier: {volumeMultiplier:0.000}.", SleepModeVolumeMultiplier);
            }

            UpdateVolume();

            await Task.Delay(1000);
        }
    }

    public void IncreaseSleepTimer()
    {
        IncreaseSleepTimer(DefaultSleepTimerDelta);
    }

    public void DecreaseSleepTimer()
    {
        IncreaseSleepTimer(-DefaultSleepTimerDelta);
    }
    
    public void IncreaseSleepTimer(TimeSpan timeLeftToSleepActivationDelta)
    {
        TimeSpan newTimeLeftToSleepActivation = TimeLeftToSleepActivation;

        if (newTimeLeftToSleepActivation < TimeSpan.Zero)
        {
            newTimeLeftToSleepActivation = TimeSpan.Zero;
        }

        newTimeLeftToSleepActivation = newTimeLeftToSleepActivation + timeLeftToSleepActivationDelta;

        if (newTimeLeftToSleepActivation > TimeSpan.Zero)
        {
            StartSleepTimer(newTimeLeftToSleepActivation);
        }
    }

    public void StopSleepTimer()
    {
        Logger.LogInformation("User stops sleep timer.");

        IsSleepTimerEnabled = false;
        SleepTimerSessionId++;
        SendSleepModeMessage();

        UpdateVolume();
    }

    private void SendSleepModeMessage()
    {
        Messenger.Send(new SleepStateChanged(IsSleepTimerEnabled, TimeLeftToSleepActivation));
    }

    #endregion
}
