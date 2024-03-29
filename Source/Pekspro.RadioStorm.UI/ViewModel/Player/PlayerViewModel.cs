﻿namespace Pekspro.RadioStorm.UI.ViewModel.Player;

public sealed partial class PlayerViewModel : ObservableObject
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
        BufferRatio = audioManager.BufferRatio;

        messenger.Register<PlayerButtonStateChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                CanPause = message.CanPause;
                CanPlay = message.CanPlay;
                CanStop = message.CanStop;
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

        messenger.Register<SleepStateChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                bool wasSleepModeRunning = IsSleepTimerRunning;

                IsSleepTimerRunning = message.IsSleepModeActivated;
                TimeLeftToSleepActivation = message.TimeLeftToSleepActivation;

                if (wasSleepModeRunning != IsSleepTimerRunning && !IsSleepTimerRunning)
                {
                    SleepTimeIndex = 0;
                }
            });
        });

        messenger.Register<BufferRatioChanged>(this, (sender, message) =>
        {
            MainThreadRunner.RunInMainThread(() =>
            {
                BufferRatio = message.BufferRatio;
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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    private bool _CanStop;

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

    public string MediaLengthString => CreatePositionString(MediaLength);

    public bool IsMediaLengthKnown
    {
        get
        {
            if (IsBuffering)
            {
                return false;
            }

            return MediaLength.Ticks > 0;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BufferPercent))]
    private double? _BufferRatio;

    public int? BufferPercent => BufferRatio is null ? null : (int)(BufferRatio * 100);

    #endregion

    #region Playlist properties

    public PlayListItem? CurrentItem => CurrentPlayList?.CurrentItem;

    public bool IsCurrentItemLiveAudio => CurrentItem?.IsLiveAudio == true;

    public bool IsCurrentItemEpisode => CurrentItem?.IsLiveAudio == false;

    public bool HasPlayList => CurrentPlayList?.Items.Count > 1;

    [ObservableProperty]
    private PlayList? _CurrentPlayList;

    public bool HasMorePlayListItems =>
        AudioManager.CurrentPlayList?.CanGoToNext == true;

    public int PlayListItemCount =>
        AudioManager.CurrentPlayList?.Items.Count ?? 1;

    public int PlayListItemIndex =>
        (AudioManager.CurrentPlayList?.CurrentPosition ?? 0) + 1;


    #endregion

    #region Volume

    public double Volume
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

    partial void OnSleepTimeIndexChanged(int value)
    {
        if (value == 0 || value >= SleepTimesInMinutes.Length)
        {
            return;
        }
        else
        {
            AudioManager.StartSleepTimer(TimeSpan.FromMinutes(SleepTimesInMinutes[value - 1]));
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSleepTimerNotRunning))]
    [NotifyCanExecuteChangedFor(nameof(StopSleepTimerCommand))]
    private bool _IsSleepTimerRunning;

    public bool IsSleepTimerNotRunning => !IsSleepTimerRunning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SleepTimerText))]
    [NotifyPropertyChangedFor(nameof(CanSleepTimerDecrease))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseSleepTimerCommand))]
    private TimeSpan _TimeLeftToSleepActivation;

    public bool CanSleepTimerDecrease => TimeLeftToSleepActivation > AudioManagerBase.DefaultSleepTimerDelta;

    public string SleepTimerText => string.Format("{0:00}:{1:00}", (int) TimeLeftToSleepActivation.TotalMinutes, TimeLeftToSleepActivation.Seconds);

    #endregion

    #region Speed

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNormalPlaybackRate))]
    [NotifyPropertyChangedFor(nameof(PlaybackRateShortString))]

    private int _PlaybackRateIndex;

    public bool HasNormalPlaybackRate => PlaybackRateIndex == 2;
    
    public string PlaybackRateShortString => AudioManager.PlaybackRate.ToString("0.00").Replace(".", ",").TrimEnd('0').TrimEnd(',') + "x";

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

    [RelayCommand(CanExecute = nameof(CanStop))]
    public void Stop()
    {
        AudioManager.Stop();
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
    public void SetSpeedVerySlow()
    {
        AudioManager.PlaybackRateIndex = 0;
    }

    [RelayCommand]
    public void SetSpeedSlow()
    {
        AudioManager.PlaybackRateIndex = 1;
    }

    [RelayCommand]
    public void SetSpeedNormal()
    {
        AudioManager.PlaybackRateIndex = 2;
    }
    
    [RelayCommand]
    public void SetSpeedFast()
    {
        AudioManager.PlaybackRateIndex = 3;
    }
    
    [RelayCommand]
    public void SetSpeedVeryFast()
    {
        AudioManager.PlaybackRateIndex = 4;
    }
    
    [RelayCommand]
    public void ToggleSleepTimer()
    {
        if (AudioManager.IsSleepTimerEnabled)
        {
            AudioManager.StopSleepTimer();
        }
        else
        {
            AudioManager.StartSleepTimer();
        }
    }

    [RelayCommand]
    public void StopSleepTimer()
    {
        AudioManager.StopSleepTimer();
    }

    [RelayCommand]
    public void IncreaseSleepTimer()
    {
        AudioManager.IncreaseSleepTimer();
    }

    [RelayCommand(CanExecute = nameof(CanSleepTimerDecrease))]
    public void DecreaseSleepTimer()
    {
        AudioManager.DecreaseSleepTimer();
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
        OnPropertyChanged(nameof(HasMorePlayListItems));
        OnPropertyChanged(nameof(PlayListItemIndex));
        OnPropertyChanged(nameof(PlayListItemCount));
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
