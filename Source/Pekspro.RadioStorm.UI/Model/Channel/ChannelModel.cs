using Microsoft.Extensions.Logging;

namespace Pekspro.RadioStorm.UI.Model.Channel;

public partial class ChannelModel : FavoriteBaseModel
{
    #region Private properties

    private IUriLauncher UriLauncher { get; }

    private IAudioManager AudioManager { get; }

    private IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructors

    internal static ChannelModel CreateWithSampleData(int sampleType = 0)
    {
        ChannelData channelData = SampleData.ChannelDataSample(sampleType);
        ChannelStatusData status = SampleData.ChannelStatusDataSample(sampleType);

        var model = new ChannelModel(null!, null!, null!, null!, null!, channelData, null!);
        model.SetStatus(status);

        return model;
    }

    public ChannelModel()
        : this(null!, null!, null!, null!, null!, SampleData.ChannelDataSample(0), null!)
    {
        SetStatus(SampleData.ChannelStatusDataSample(0));
    }

    public ChannelModel(
        IChannelFavoriteList channelFavoriteList,
        IUriLauncher uriLauncher,
        IMessenger messenger,
        IAudioManager audioManager,
        IDateTimeProvider dateTimeProvider,
        string name,
        ILogger logger
        )
        : base(channelFavoriteList, name, logger)
    {
        messenger?.Register<ChannelFavoriteChangedMessage>(this, (r, m) =>
        {
            if (m?.Id is null || m.Id == Id)
            {
                OnPropertyChanged(nameof(IsFavorite));
            }
        }
        );

        messenger?.Register<PlaylistChanged>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(IsPlayingThis));
            OnPropertyChanged(nameof(AudioMediaState));
        }
        );

        messenger?.Register<PlayerButtonStateChanged>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(IsPlayingThis));
            OnPropertyChanged(nameof(AudioMediaState));
        }
        );

        UriLauncher = uriLauncher;
        AudioManager = audioManager;
        DateTimeProvider = dateTimeProvider;
    }

    public ChannelModel(
        IChannelFavoriteList channelFavoriteList,
        IUriLauncher uriLauncher,
        IMessenger messenger,
        IAudioManager audioManager,
        IDateTimeProvider dateTimeProvider,
        ChannelData channelData,
        ILogger logger
    )
        : this(channelFavoriteList, uriLauncher, messenger, audioManager, dateTimeProvider, channelData.Title, logger)
    {
        Init(channelData);
    }

    private void Init(ChannelData c)
    {
        Id = c.ChannelId;
        ChannelGroupName = c.ChannelGroupName;
        WebPageUri = c.WebPageUri;
        LiveAudioUrl = c.LiveAudioUrl;
        Title = c.Title;
        ChannelImage = new ImageLink.ImageLink(c.ChannelImageHighResolution, c.ChannelImageLowResolution);

        if (c.ChannelColor is null)
        {
            Logger.LogInformation("Will ignore bad channel color: {0}", c.ChannelColor);
        }
        else if (c.ChannelColor.Length == 6 && int.TryParse(c.ChannelColor, System.Globalization.NumberStyles.HexNumber, null, out int _))
        {
            ChannelColor = "#" + c.ChannelColor;
        }
        else
        {
            Logger.LogInformation("Will ignore bad channel color: {0}", c.ChannelColor);
        }

        ChannelGroupPriority = GetChannelGroupPriority(ChannelGroupName);
    }

    public static int GetChannelGroupPriority(string channelGroupName)
    {
        if (channelGroupName.Contains("iks"))
        {
            return 1;
        }
        else if (channelGroupName.Contains("okal"))
        {
            return 2;
        }
        else if (channelGroupName.Contains("ler k"))
        {
            return 3;
        }
        else if (channelGroupName.Contains("inori"))
        {
            return 4;
        }
        else if (channelGroupName.Contains("xtra"))
        {
            return 10;
        }
        else
        {
            return 5;
        }
    }

    private void SetStatus(ChannelStatusData? channelStatusData)
    {
        if (channelStatusData is not null)
        {
            Status = new ChannelStatusModel(channelStatusData, DateTimeProvider, ChannelImage);
        }
        else
        {
            Status = null;
        }
    }

    #endregion

    #region Properites

    public string Title { get; private set; } = string.Empty;

    public string ChannelGroupName { get; private set; } = string.Empty;

    public int ChannelGroupPriority { get; private set; }

    public string? WebPageUri { get; private set; }

    public bool HasWebPage => !string.IsNullOrEmpty(WebPageUri);

    [ObservableProperty]
    private string _ChannelColor = string.Empty;

    public bool IsPlayingThis => AudioManager is not null && AudioManager.IsItemIdActive(true, Id);

    public bool CanPause => AudioManager.CanPause && AudioManager.IsItemIdActive(true, Id);

    public MediaState AudioMediaState => CanPause ? MediaState.CanPause : MediaState.CanPlay;

    [ObservableProperty]
    private string? _LiveAudioUrl;

    [ObservableProperty]
    private ChannelStatusModel? _Status = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Image))]
    private ImageLink.ImageLink? _ChannelImage;

    public ImageLink.ImageLink? Image => Status?.CurrentProgramImage ?? ChannelImage;

    #endregion

    #region Commands

    [RelayCommand]
    private void Play()
    {
        AudioManager.Play(new PlayListItem()
        {
            AudioId = Id,
            Channel = Title,
            StreamUrl = LiveAudioUrl,
            Program = Status?.CurrentProgram,
            IconUri = ChannelImage?.HighResolution,
            IsLiveAudio = true
        });
    }

    [RelayCommand]
    private void PlayPause()
    {
        bool isCurrentItemThisChannel = AudioManager.IsItemIdActive(true, Id);

        if (
            AudioManager.CanPause && isCurrentItemThisChannel
            )
        {
            AudioManager.Pause();
        }
        else
        {
            if (isCurrentItemThisChannel)
            {
                AudioManager.Play();
            }
            else
            {
                AudioManager.Play(new PlayListItem()
                {
                    AudioId = Id,
                    Channel = Title,
                    StreamUrl = LiveAudioUrl,
                    Program = Status?.CurrentProgram,
                    IconUri = ChannelImage?.HighResolution,
                    IsLiveAudio = true
                });
            }
        }
    }

    [RelayCommand(CanExecute = nameof(HasWebPage))]
    private void OpenWebPage()
    {
        if (!string.IsNullOrWhiteSpace(WebPageUri))
        {
            _ = UriLauncher.LaunchAsync(WebPageUri);
        }
    }

    #endregion
}
