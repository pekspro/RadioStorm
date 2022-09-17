namespace Pekspro.RadioStorm.UI.ViewModel.Player;

public partial class PlayerViewModel : ObservableObject
{
    #region Private properties

    private static readonly int[] SleepTimesInMinutes = new[]
    {
        5,
        10,
        15,
        30,
        60,
        120
    };

    private IAudioManager AudioManager { get; }
    private IMainThreadRunner MainThreadRunner { get; }

    #endregion

    #region Constructor

    public PlayerViewModel()
    {
        CanPause = false;
        CanPlay = true;
        IsBuffering = false;

        AudioManager = null!;
        MainThreadRunner = null!;

        AudioPosition = TimeSpan.FromSeconds(76);
        MediaLength = TimeSpan.FromSeconds(120);
    }

    public PlayerViewModel(IAudioManager audioManager, IMainThreadRunner mainThreadRunner,
        IMessenger messenger)
    {
        AudioManager = audioManager;
        MainThreadRunner = mainThreadRunner;

        CanPause = audioManager.CanPause;
        CanPlay = audioManager.CanPlay;
        IsBuffering = audioManager.IsBuffering;

        _PlaybackRateIndex = audioManager.PlaybackRateIndex;
        AudioPosition = audioManager.Position;
        MediaLength = audioManager.MediaLength;

        messenger.Register<PlayerButtonStateChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                CanPause = message.CanPause;
                CanPlay = message.CanPlay;
                IsBuffering = message.IsBuffering;
            });
        });

        messenger.Register<MediaPositionChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                LatestDraggingPosition = null;
                AudioPosition = message.Position;
                MediaLength = message.Length;
            });
        });

        messenger.Register<PlaylistChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                CurrentPlayList = message.PlayList;
                PlaylistChanged();
            });
        });

        messenger.Register<CurrentItemChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                CurrentPlayList = message.PlayList;
                PlaylistChanged();
            });
        });

        messenger.Register<SpeedRateChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                PlaybackRateIndex = message.PlaybackRateIndex;
            });
        });
    }

    #endregion

    #region Properites

    #region SeekSize

    public SeekSizeViewModel SeekSizeViewModel { get; } = new SeekSizeViewModel();

    #endregion

    #region Button properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlayPause))]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    private bool _CanPlay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlayPause))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(PlayPauseCommand))]
    private bool _CanPause;

    public bool CanPlayPause => CanPause || CanPlay;

    public bool CanGoToNext => CurrentPlayList?.CanGoToNext == true;

    public bool CanGoToPrevious => CurrentPlayList?.CanGoToPrevious == true;

    #endregion

    #region Status properties

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMediaLengthKnown))]
    private bool _IsBuffering;

    #region AudioPosition property

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositionString))]
    private TimeSpan _AudioPosition;

    #endregion

    #region DraggingPosition property

    private TimeSpan? LatestDraggingPosition { get; set; }

    private TimeSpan? _DraggingPosition;

    public TimeSpan? DraggingPosition
    {
        get
        {
            return _DraggingPosition;
        }
        set
        {
            if (value is not null)
            {
                LatestDraggingPosition = value;
            }

            if (SetProperty(ref _DraggingPosition, value))
            {
                OnPropertyChanged(nameof(PositionString));
            }
        }
    }

    #endregion

    public TimeSpan Position => DraggingPosition ?? LatestDraggingPosition ?? AudioPosition;

    public string PositionString => CreatePositionString(Position);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MediaLengthString))]
    [NotifyPropertyChangedFor(nameof(IsMediaLengthKnown))]
    private TimeSpan _MediaLength;

    public string MediaLengthString => CreatePositionString(_MediaLength);

    public bool IsMediaLengthKnown
    {
        get
        {
            if (IsBuffering)
            {
                return false;
            }

            return _MediaLength.Ticks > 0;
        }
    }

    #endregion

    #region Playlist properties

    public PlayListItem? CurrentItem => CurrentPlayList?.CurrentItem;

    public bool IsCurrentItemLiveAudio => CurrentItem?.IsLiveAudio == true;

    public bool IsCurrentItemEpisode => CurrentItem?.IsLiveAudio == false;

    public bool HasPlayList => CurrentPlayList?.Items.Count > 1;

    [ObservableProperty]
    private PlayList? _CurrentPlayList;

    #endregion

    #region Volume

    public int Volume
    {
        get
        {
            return AudioManager.Volume;
        }
        set
        {
            AudioManager.Volume = value;
        }
    }

    public bool HasVolmeSupport => AudioManager.HasVolumeSupport;

    #endregion

    #region Sleep time properties

    private List<string>? _SleepTimes;

    public IReadOnlyList<string> SleepTimes
    {
        get
        {
            if (_SleepTimes is null)
	        {       
                _SleepTimes = SleepTimesInMinutes.Select(time => string.Format(Strings.Player_MenuSleepTimer_Set, time)).ToList();

                _SleepTimes.Insert(0, Strings.Player_MenuSleepTimer);

            }

            return _SleepTimes;
        }
    }

    [ObservableProperty]
    private int _SleepTimeIndex;

    async partial void OnSleepTimeIndexChanged(int value)
    {
        if (value == 0 || value >= SleepTimesInMinutes.Length)
        {
            return;
        }
        else
        {
            IsSleepTimerRunning = true;

            SleepTimeLeftInSeconds = SleepTimesInMinutes[value - 1] * 10;

            while (SleepTimeLeftInSeconds > 0)
            {
                await Task.Delay(50);
                SleepTimeLeftInSeconds--;
            }

            IsSleepTimerRunning = false;
            SleepTimeIndex = 0;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSleepTimerNotRunning))]
    [NotifyCanExecuteChangedFor(nameof(StopSleepTimerCommand))]
    private bool _IsSleepTimerRunning;

    public bool IsSleepTimerNotRunning => !IsSleepTimerRunning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StopSleepTimerText))]
    private int _SleepTimeLeftInSeconds;

    public string StopSleepTimerText =>
        string.Format(Strings.Player_MenuSleepTimer_Disable, SleepTimeLeftInSeconds / 60, SleepTimeLeftInSeconds % 60);

    #endregion

    #region Speed

    private List<string>? _PlaybackRateOptions;

    public IReadOnlyList<string> PlaybackRateOptions =>
        _PlaybackRateOptions ??
            (_PlaybackRateOptions = new List<string>()
            {
                Strings.Player_MenuSpeed_VerySlow,
                Strings.Player_MenuSpeed_Slow,
                Strings.Player_MenuSpeed_Normal,
                Strings.Player_MenuSpeed_Fast,
                Strings.Player_MenuSpeed_VeryFast,
            });

    [ObservableProperty]
    private int _PlaybackRateIndex;

    partial void OnPlaybackRateIndexChanged(int value)
    {
        AudioManager.PlaybackRateIndex = value;
    }

    #endregion

    #region Menu

    [ObservableProperty]
    private bool _IsMenuOpen;

    #endregion

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(CanPlay))]
    public void Play()
    {
        AudioManager.Play();
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    public void Pause()
    {
        AudioManager.Pause();
    }

    [RelayCommand(CanExecute = nameof(CanPlayPause))]
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

    [RelayCommand]
    public void Rewind()
    {
        AudioManager.Move(SeekSizeViewModel.RewindSize);
        SeekSizeViewModel.Decrease();
    }

    [RelayCommand]
    public void Forward()
    {
        AudioManager.Move(SeekSizeViewModel.ForwardSize);
        SeekSizeViewModel.Increase();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNext))]
    public void GoToNext()
    {
        AudioManager.GoToNext();
    }

    [RelayCommand(CanExecute = nameof(CanGoToPrevious))]
    public void GoToPrevious()
    {
        AudioManager.GoToPrevious();
    }

    [RelayCommand]
    public void StopSleepTimer()
    {
        SleepTimeIndex = 0;
        SleepTimeLeftInSeconds = 0;
        IsSleepTimerRunning = false;
    }

    [RelayCommand]
    public void ToogleMenu()
    {
        IsMenuOpen = !IsMenuOpen;
    }

    #endregion

    #region Methods

    private void PlaylistChanged()
    {
        OnPropertyChanged(nameof(CanGoToNext));
        OnPropertyChanged(nameof(CanGoToPrevious));
        OnPropertyChanged(nameof(CurrentItem));
        OnPropertyChanged(nameof(IsCurrentItemEpisode));
        OnPropertyChanged(nameof(IsCurrentItemLiveAudio));
        OnPropertyChanged(nameof(HasPlayList));
        GoToNextCommand.NotifyCanExecuteChanged();
        GoToPreviousCommand.NotifyCanExecuteChanged();
    }

    public void SeekPosition(TimeSpan position)
    {
        AudioManager.SetPlaybackPosition(position);
        AudioPosition = AudioManager.Position;
    }

    public string CreatePositionString(TimeSpan position) =>
        position.ToString("hh\\:mm\\:ss");
    
    #endregion
}
