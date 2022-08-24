namespace Pekspro.RadioStorm.Audio;

public abstract class AudioManagerBase : IAudioManager
{
    #region Private properties
    
    private IRecentPlayedManager RecentPlayedManager { get; }

    private IDownloadManager DownloadManager { get; }

    private ILocalSettings LocalSettings { get; }

    private System.Timers.Timer? RefreshPositionTimer;

    private IMainThreadTimer? MainThreadRefreshPositionTimer;

    private IListenStateManager ListenStateManager { get; }

    private IMessenger Messenger { get; }

    public ILogger Logger { get; }

    private enum RestorePostionMode { None, Restore, RestoreAtAnyMargin };

    private RestorePostionMode RestorePosition;

    private int LatestOnLengthAndPositionUpdatedItemAudioId = -1;

    private bool LatestOnLengthAndPositionUpdatedTriedSetToTrue = false;

    private SeekSizeProvider SeekSizeProvider = new SeekSizeProvider();
    
    #endregion

    #region Abstract

    protected abstract void MediaPlay(PlayListItem playlistItem);
    protected abstract void MediaPlay();
    protected abstract void MediaPause();
    protected abstract void MediaSetPlaybackPosition(TimeSpan position);
    protected abstract void MediaRefreshButtonStates();
    protected abstract void MediaRefreshLengthAndPosition();
    abstract protected void MediaSetVolume(int volume);
    abstract public bool HasVolumeSupport { get; }

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
            ILogger logger,
            bool useMainThreadTimer
        )
    {
        RecentPlayedManager = recentPlayedManager;
        DownloadManager = downloadManager;
        LocalSettings = localSettings;
        ListenStateManager = listenStateManager;
        Messenger = messenger;
        Logger = logger;
        
        if (HasVolumeSupport)
        {
            Volume = localSettings.Volume;
        }

        if (useMainThreadTimer)
        {
            MainThreadRefreshPositionTimer = mainThreadTimerFactory.CreateTimer("Audio refresh");
            MainThreadRefreshPositionTimer.Interval = 1000;
            MainThreadRefreshPositionTimer.SetupCallBack(OnTimerTick);
        }
        else
        {
            RefreshPositionTimer = new ();
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

    public bool CanGoToNext { get; set; }

    public bool CanGoToPrevious { get; set; }

    public bool IsBuffering { get; set; }

    public TimeSpan Position { get; set; }

    public TimeSpan MediaLength { get; set; }

    public int ProgressMaxValue { get; set; }

    public int ProgressValue { get; set; }

    #region Volume property

    private int _Volume;

    public int Volume
    {
        get
        {
            return _Volume;
        }
        set
        {
            _Volume = value;
            LocalSettings.Volume = _Volume;
            MediaSetVolume(_Volume);
        }
    }

    #endregion

    public PlayListItem? CurrentItem => CurrentPlayList?.CurrentItem;

    public PlayList? CurrentPlayList { get; set; }

    #endregion

    #region Methods

    public void Play()
    {
        // File just downloaded?
        if (TryUpdateCurrentItemDownloadedFile())
        {
            // Switch url
            Logger.LogInformation($"Switches to audio url/path: {CurrentItem!.PreferablePlayUrl}");
            MediaPlay(CurrentItem);
            RestorePosition = RestorePostionMode.RestoreAtAnyMargin;
        }
        else
        {
            MediaPlay();
        }
    }

    public void Pause()
    {
        if (CanPause)
        {
            MediaPause();
        }
    }

    public void PlayPause()
    {
        if (CanPause)
        {
            Pause();
        }
        else if (CanPlay)
        {
            Play();
        }
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
            return false;
        }

        if (CurrentPlayList.CurrentPosition + 1 >= CurrentPlayList.Items.Count)
        {
            return false;
        }

        CurrentPlayList.CurrentPosition++;
        SendCurrentItemChanged();

        SetPlaylistPosition(CurrentPlayList.CurrentPosition);

        return true;
    }

    public bool GoToPrevious()
    {
        if (CurrentPlayList is null)
        {
            return false;
        }

        if (CurrentPlayList.CurrentPosition <= 0)
        {
            return false;
        }

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
        MediaPlay(CurrentPlayList.CurrentItem!);

        RecentPlayedManager.AddOrUpdate(!CurrentItem!.IsLiveAudio, CurrentItem.AudioId);

        SendCurrentItemChanged();
    }

    public void SetPlaybackPosition(TimeSpan position)
    {
        MediaSetPlaybackPosition(position);
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
    }

    private void SendPlaylistChanged(bool itemsMoved = false)
    {
        if (CurrentPlayList is not null)
        {
            Messenger.Send(new PlaylistChanged(CurrentPlayList, itemsMoved));
        }
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
            Messenger.Send(new PlayerButtonStateChanged(CanPlay, CanPause, IsBuffering));
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

        Logger.LogInformation($"Should continue from position {nextPosition} (from listen state), margin {nextPosition}.");

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

    #endregion
}
