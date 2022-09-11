namespace Pekspro.RadioStorm.UI.Model.Episode;

[DebuggerDisplay("{PublishLength.PublishDate} {Title}")]
public partial class EpisodeModel : ObservableObject, IComparable<EpisodeModel>
{
    #region Private properties

    private IListenStateManager ListenStateManager { get; }

    private IAudioManager AudioManager { get; }

    private ILocalSettings LocalSettings { get; }

    private IMainThreadRunner MainThreadRunner { get; }
    
    private IDateTimeProvider DateTimeProvider { get; }
    
    private IWeekdaynameHelper WeekdaynameHelper { get; }

    private IDownloadManager DownloadManager { get; }
    
    #endregion

    #region Constructor

    internal static EpisodeModel CreateWithSampleData(int sampleType = 0)
    {
        var episodeData = SampleData.EpisodeDataSample(sampleType);
        var download = SampleData.DownloadSample(sampleType);

        var model = new EpisodeModel(episodeData, null!, null!, null!, null!, null!, null!, null!, null!);
        model.DownloadData = new DownloadDataViewModel(download);

        return model;
    }

    public EpisodeModel()
        : this(SampleData.EpisodeDataSample(0), null!, null!, null!, null!, null!, null!, null!, null!)
    {
        var download = SampleData.DownloadSample(0);

        DownloadData = new DownloadDataViewModel(download);
    }

    public EpisodeModel
        (
            EpisodeData c,
            IDownloadManager downloadManager,
            IListenStateManager listenStateManager,
            IAudioManager audioManager,
            ILocalSettings localSettings,
            IMainThreadRunner mainThreadRunner,
            IDateTimeProvider dateTimeProvider,
            IWeekdaynameHelper weekdaynameHelper,
            IMessenger messenger
        )
    {
        Id = c.EpisodeId;

        Title = c.Title ?? string.Empty;
        Description = c.Description ?? string.Empty;
        EpisodeImage = new ImageLink.ImageLink(c.EpisodeImage);

        ProgramName = c.ProgramName ?? string.Empty;
        ProgramId = c.ProgramId;

        AudioDownloadUrl = c.AudioDownloadUrl;
        AudioDownloadDuration = c.AudioDownloadDuration;

        AudioStreamWithoutMusicUrl = c.AudioStreamWithoutMusicUrl;
        AudioStreamWithoutMusicDuration = c.AudioStreamWithoutMusicDuration;

        AudioStreamWithMusicUrl = c.AudioStreamWithMusicUrl;
        AudioStreamWithMusicDuration = c.AudioStreamWithMusicDuration;
        AudioStreamWithMusicExpireDate = c.AudioStreamWithMusicExpireDate;

        PublishLength = new PublishLength.PublishLength(c.PublishDate, weekdaynameHelper);
        DownloadManager = downloadManager;
        ListenStateManager = listenStateManager;
        AudioManager = audioManager;
        LocalSettings = localSettings;
        MainThreadRunner = mainThreadRunner;
        DateTimeProvider = dateTimeProvider;
        WeekdaynameHelper = weekdaynameHelper;

        var download = downloadManager?.GetDownloadData(ProgramId ?? 0, Id);

        if (download is not null)
        {
            DownloadData = new DownloadDataViewModel(download);
        }

        UpdatePublishLength();

        messenger?.Register<ListenStateChangedMessage>(this, (r, m) =>
        {
            if (m?.EpisodeId is null || m.EpisodeId == Id)
            {
                OnPropertyChanged(nameof(IsListened));
            }
        }
        );

        messenger?.Register<SettingChangedMessage>(this, (r, m) =>
        {
            if (m.SettingName == nameof(LocalSettings.PreferStreamsWithMusic))
            {
                OnPropertyChanged(nameof(AudioStreamOrDownloadUrl));
                UpdatePublishLength();
            }
        }
        );

        messenger?.Register<PlaylistChanged>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(IsPlayingThis));
            OnPropertyChanged(nameof(InPlayList));
            OnPropertyChanged(nameof(CanBeAddedToPlayList));
            OnPropertyChanged(nameof(AudioMediaState));
        }
        );

        messenger?.Register<PlayerButtonStateChanged>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(IsPlayingThis));
            OnPropertyChanged(nameof(AudioMediaState));
        }
        );

        messenger?.Register<CurrentItemChanged>(this, (r, m) =>
        {
            bool playingThis = IsPlayingThis;
            OnPropertyChanged(nameof(IsPlayingThis));
            OnPropertyChanged(nameof(AudioMediaState));
        }
        );

        messenger?.Register<DownloadUpdated>(this, (r, m) =>
        {
            if (m.Download.EpisodeId == Id && m.Download.ProgramId == ProgramId)
            {
                MainThreadRunner.RunInMainThread(() =>
                {
                    if (DownloadData is null)
                    {
                        DownloadData = new DownloadDataViewModel(m.Download);
                    }
                    else
                    {
                        DownloadData.UpdateFromDownload(m.Download);
                    }

                    UpdateDownloadState();
                });
            }
        }
        );

        messenger?.Register<DownloadDeleted>(this, (r, m) =>
        {
            if (m.Download.EpisodeId == Id && m.Download.ProgramId == ProgramId)
            {
                MainThreadRunner.RunInMainThread(() =>
                {
                    DownloadData = null;

                    UpdateDownloadState();
                });
            }
        }
        );
    }

    #endregion

    #region Properties
    
    public int Id { get; }

    public string Title { get; }

    public string Description { get; }

    public PublishLength.PublishLength PublishLength { get; }

    public string PublishDateStringAndDescription => $"{PublishLength.PublishDateString} {Description}";

    public ImageLink.ImageLink EpisodeImage { get; }

    [ObservableProperty]
    private ProgramModel? _ProgramDetails;

    public string ProgramName { get; }

    public int? ProgramId { get; }

    public DateTimeOffset? AudioStreamWithMusicExpireDate { get; }

    public bool HasExpireDateNote => AudioStreamWithMusicExpireDate.HasValue;

    public string ExpireDateNote
    {
        get
        {
            if (!AudioStreamWithMusicExpireDate.HasValue)
            {
                return string.Empty;
            }

            string withMusicString = string.Empty;

            if (!string.IsNullOrEmpty(AudioStreamWithoutMusicUrl) || !string.IsNullOrEmpty(AudioDownloadUrl))
            {
                withMusicString = Strings.Episodes_ExpireNote_WithMusic;
            }

            int dayCountDiff = (AudioStreamWithMusicExpireDate.Value - DateTimeProvider.OffsetNow).Days;
            var localExpireDate = AudioStreamWithMusicExpireDate.Value.LocalDateTime;

            if (dayCountDiff == 0 || dayCountDiff == 1)
            {
                string s = Strings.Episodes_ExpireNote_OneDayOrLess;

                (string name, _) = WeekdaynameHelper.GetRelativeWeekdayName(localExpireDate, false, false, true);

                string expireString = name.ToLower();
                s = string.Format(s, expireString, withMusicString);

                return s.Replace("  ", " ").Trim() + ".";
            }
            else
            {
                string s = Strings.Episodes_ExpireNote_MultipleDays;

                s = string.Format(s, dayCountDiff, withMusicString);

                return s.Replace("  ", " ").Trim() + ".";
            }
        }
    }

    public string? AudioDownloadUrl { get; }

    public int AudioDownloadDuration { get; }

    public string? AudioStreamWithoutMusicUrl { get; }

    public int AudioStreamWithoutMusicDuration { get; }

    public string? AudioStreamWithMusicUrl { get; }

    public int AudioStreamWithMusicDuration { get; }

    public bool HasAudio => !string.IsNullOrEmpty(AudioStreamOrDownloadUrl);

    public string? AudioStreamOrDownloadUrl
    {
        get
        {
            if (LocalSettings?.PreferStreamsWithMusic == true)
            {
                if (!string.IsNullOrWhiteSpace(AudioStreamWithMusicUrl))
                {
                    return AudioStreamWithMusicUrl;
                }

                if (!string.IsNullOrWhiteSpace(AudioStreamWithoutMusicUrl))
                {
                    return AudioStreamWithoutMusicUrl;
                }

                if (!string.IsNullOrWhiteSpace(AudioDownloadUrl))
                {
                    return AudioDownloadUrl;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(AudioStreamWithoutMusicUrl))
                {
                    return AudioStreamWithoutMusicUrl;
                }

                if (!string.IsNullOrWhiteSpace(AudioDownloadUrl))
                {
                    return AudioDownloadUrl;
                }

                if (!string.IsNullOrWhiteSpace(AudioStreamWithMusicUrl))
                {
                    return AudioStreamWithMusicUrl;
                }
            }


            return null;
        }
    }

    //private string? AudioDownloadOrStreamUrl
    //    => AudioUrlHelper.GetDownloadUrl(AudioStreamWithMusicUrl, AudioStreamWithoutMusicUrl, AudioDownloadUrl);

    public bool IsPlayingThis => AudioManager?.IsItemIdActive(false, Id) == true;

    public bool InPlayList => AudioManager?.IsItemIdInPlayList(false, Id) == true;

    public bool CanBeAddedToPlayList => HasAudio && !InPlayList;

    [ObservableProperty]
    private DownloadDataViewModel? _DownloadData;

    public bool NoDownloadAvailible => (HasAudio && !HasDownloadSupport);

    public bool HasDownloadSupport => !string.IsNullOrEmpty(AudioDownloadUrl);

    public bool HasDownloadSupportAndNoDownload => HasDownloadSupport && (DownloadData is null);

    public bool CanDownload =>
        HasDownloadSupport && (DownloadData is null || DownloadData.Status == DownloadDataStatus.Error);

    public bool CanDeleteDownload =>
        HasDownloadSupport && DownloadData is not null;

    public bool CanPause
    {
        get
        {
            if (AudioManager is null)
            {
                return false;
            }

            if (AudioManager.CanPause && AudioManager.IsItemIdActive(false, Id))
            {
                return true;
            }

            return false;
        }
    }

    public MediaState AudioMediaState
    {
        get
        {
            if (!HasAudio)
            {
                return MediaState.Disabled;
            }

            if (IsPlayingThis)
            {
                if (AudioManager.CanPause)
                {
                    return MediaState.CanPause;
                }

                return MediaState.CanPlay;
            }

            return MediaState.CanPlay;
        }
    }

    public bool SetAsActivePlaylistItem { get; internal set; }

    public bool IsListened
    {
        get
        {
            return ListenStateManager?.IsFullyListen(Id) == true;
        }
        set
        {
            ListenStateManager?.SetFullyListen(Id, value);
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ToggleIsListened()
    {
        bool isFullyListened = ListenStateManager.IsFullyListen(Id);
        ListenStateManager.SetFullyListen(Id, !isFullyListened);
    }


    [RelayCommand(CanExecute = nameof(CanDownload))]
    public void Download()
    {
        string? url = AudioDownloadUrl;

        if (!string.IsNullOrEmpty(url) && HasDownloadSupport)
        {
            DownloadManager.StartDownload(ProgramId ?? 0, Id, url, true, ProgramName, Title);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteDownload))]
    public void DeleteDownload()
    {
        DownloadManager.DeleteDownload(ProgramId ?? 0, Id);
    }

    [RelayCommand]
    public void AddToPlayList()
    {
        AudioManager.Add(CreatePlayListItem());
    }

    [RelayCommand(CanExecute = nameof(HasAudio))]
    private void Play()
    {
        if (HasAudio)
        {
            int? playListPos = AudioManager.CurrentPlayList?.Items.FindIndex(a => a.IsLiveAudio == false && a.AudioId == Id);

            if (SetAsActivePlaylistItem && playListPos >= 0)
            {
                AudioManager.SetPlaylistPosition(playListPos.Value);
            }
            else if (IsPlayingThis)
            {
                //Looks like we want to continue on a local file.
                //Make sure we continue on current position.
                var item = CreatePlayListItem();
                item.NextPlayPosition = (int)AudioManager.Position.TotalSeconds;

                AudioManager.Play(item);
            }
            else
            {
                AudioManager.Play(CreatePlayListItem());
            }
        }
    }

    [RelayCommand(CanExecute = nameof(HasAudio))]
    private void PlayPause()
    {
        bool isCurrentItemThisEpisode = AudioManager.IsItemIdActive(false, Id);


        if (
            AudioManager.CanPause && isCurrentItemThisEpisode
            )
        {
            AudioManager.Pause();
        }
        else
        {
            if (isCurrentItemThisEpisode)
            {
                AudioManager.Play();
            }
            else
            {
                Play();
            }
        }
    }

    #endregion

    #region Methods

    public void UpdateMediaState()
    {
        OnPropertyChanged(nameof(CanPause));
        OnPropertyChanged(nameof(IsPlayingThis));
        OnPropertyChanged(nameof(AudioMediaState));
        OnPropertyChanged(nameof(HasAudio));
        PlayCommand.NotifyCanExecuteChanged();
        PlayPauseCommand.NotifyCanExecuteChanged();
    }

    public void UpdateDownloadState()
    {
        OnPropertyChanged(nameof(HasDownloadSupport));
        OnPropertyChanged(nameof(HasDownloadSupportAndNoDownload));
        OnPropertyChanged(nameof(NoDownloadAvailible));
        OnPropertyChanged(nameof(HasExpireDateNote));
        OnPropertyChanged(nameof(ExpireDateNote));
        OnPropertyChanged(nameof(CanDownload));
        OnPropertyChanged(nameof(CanDeleteDownload));
        DeleteDownloadCommand.NotifyCanExecuteChanged();
        DownloadCommand.NotifyCanExecuteChanged();
    }

    private void UpdatePublishLength()
    {
        if (LocalSettings?.PreferStreamsWithMusic == true)
        {
            if (AudioStreamWithMusicDuration > 0)
            {
                if (PublishLength.Length.TotalSeconds != AudioStreamWithMusicDuration)
                {
                    PublishLength.Length = new TimeSpan(0, 0, AudioStreamWithMusicDuration);
                }
            }
            else if (AudioStreamWithoutMusicDuration > 0)
            {
                if (PublishLength.Length.TotalSeconds != AudioStreamWithoutMusicDuration)
                {
                    PublishLength.Length = new TimeSpan(0, 0, AudioStreamWithoutMusicDuration);
                }
            }
            else if (AudioDownloadDuration > 0)
            {
                if (PublishLength.Length.TotalSeconds != AudioDownloadDuration)
                {
                    PublishLength.Length = new TimeSpan(0, 0, AudioDownloadDuration);
                }
            }
        }
        else
        {
            if (AudioStreamWithoutMusicDuration > 0)
            {
                if (PublishLength.Length.TotalSeconds != AudioStreamWithoutMusicDuration)
                {
                    PublishLength.Length = new TimeSpan(0, 0, AudioStreamWithoutMusicDuration);
                }
            }
            else if (AudioDownloadDuration > 0)
            {
                if (PublishLength.Length.TotalSeconds != AudioDownloadDuration)
                {
                    PublishLength.Length = new TimeSpan(0, 0, AudioDownloadDuration);
                }
            }
            else if (AudioStreamWithMusicDuration > 0)
            {
                if (PublishLength.Length.TotalSeconds != AudioStreamWithMusicDuration)
                {
                    PublishLength.Length = new TimeSpan(0, 0, AudioStreamWithMusicDuration);
                }
            }
        }
    }

    public PlayListItem CreatePlayListItem()
    {
        return new PlayListItem()
        {
            AudioId = Id,
            Channel = null,
            Episode = Title,
            StreamUrl = AudioStreamOrDownloadUrl,
            Program = ProgramName,
            ProgramId = ProgramId ?? 0,
            IconUri = EpisodeImage.HighResolution,
            IsLiveAudio = false
        };
    }

    public int CompareTo(EpisodeModel? other)
    {
        if (!PublishLength.PublishDate.HasValue)
        {
            return -1;
        }

        if (other is null)
        {
            return 1;
        }

        if (!other.PublishLength.PublishDate.HasValue)
        {
            return 1;
        }

        return PublishLength.PublishDate.Value.CompareTo(other.PublishLength.PublishDate.Value);
    }

    #endregion
}
