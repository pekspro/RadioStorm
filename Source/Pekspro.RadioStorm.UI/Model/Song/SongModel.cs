using TextCopy;

namespace Pekspro.RadioStorm.UI.Model.Song;

public partial class SongModel : ObservableObject, IEquatable<SongModel>
{
    #region Private properties
    
    private IAudioManager AudioManager { get; }
    
    private ILogger Logger { get; }

    private EpisodeData? EpisodeData { get; }

    #endregion

    #region Constructor

    internal static SongModel CreateWithSampleData(int sampleType = 0)
    {
        var songData = SampleData.SongListItemDataSample(sampleType);
        var episodeData = SampleData.EpisodeDataSample(sampleType);

        var model = new SongModel(songData, episodeData, null!, null!, null!);

        return model;
    }

    public SongModel()
        : this(SampleData.SongListItemDataSample(0), SampleData.EpisodeDataSample(0), null!, null!, null!)
    {

    }

    public SongModel
        (
            SongListItemData data, 
            EpisodeData? episodeData, 
            IAudioManager audioManager,
            IWeekdaynameHelper weekdaynameHelper,
            ILogger<SongModel> logger
        )
    {
        Title = data.Title ?? string.Empty;
        Artist = data.Artist ?? string.Empty;
        AlbumName = data.AlbumName ?? string.Empty;
        Composer = data.Composer ?? string.Empty;
        PublishDate = new DateTimeHolder.DateTimeHolder(data.PublishDate, weekdaynameHelper);
        EpisodeData = episodeData;
        AudioManager = audioManager;
        Logger = logger;
    }

    #endregion

    #region Properties
    
    public string Title { get; }
    
    public string Artist { get; }
    
    public string AlbumName { get; }
    
    public string Composer { get; }
    
    public DateTimeHolder.DateTimeHolder PublishDate { get; }
    
    public bool IsPlayable => StartOffset > 0;

    public int StartOffset
    {
        get
        {
            if (EpisodeData?.AudioStreamWithMusicUrl is null ||
                PublishDate?.Date is null)
            {
                return -1;
            }

            TimeSpan offset = PublishDate.Date.Value - EpisodeData.PublishDate;

            if (offset.TotalSeconds >= 0 && offset.TotalSeconds < EpisodeData.AudioStreamWithMusicDuration)
            {
                return (int)offset.TotalSeconds;
            }

            return -1;
        }
    }

    #endregion

    #region Commands
    
    [RelayCommand]
    private void Copy()
    {
        try
        {
            string text = $"{Title} - {Artist}";

            ClipboardService.SetText(text);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error copying content to Clipboard: " + ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(IsPlayable))]
    private void Play()
    {
        if (IsPlayable)
        {
            if (AudioManager.CurrentItem is not null &&
                EpisodeData is not null &&
                AudioManager.CurrentItem.AudioId == EpisodeData.EpisodeId &&
                AudioManager.CurrentItem.PreferablePlayUrl == EpisodeData.AudioStreamWithMusicUrl
                )
            {
                AudioManager.ProgressValue = StartOffset;
                AudioManager.Play();
            }
            else
            {
                var item = CreatePlayListItem();
                item.NextPlayPosition = StartOffset;

                AudioManager.Play(item);
            }
        }
    }

    #endregion

    #region Methods
    
    public PlayListItem CreatePlayListItem()
    {
        return new PlayListItem()
        {
            AudioId = EpisodeData!.EpisodeId,
            Channel = null,
            Episode = EpisodeData.Title,
            StreamUrl = EpisodeData.AudioStreamWithMusicUrl,
            Program = EpisodeData.ProgramName,
            ProgramId = EpisodeData.ProgramId ?? 0,
            IconUri = EpisodeData.EpisodeImage,
            IsLiveAudio = false
        };
    }

    public bool Equals(SongModel? other)
    {
        if (other is null)
        {
            return false;
        }

        return (
                    AlbumName == other.AlbumName &&
                    Artist == other.Artist &&
                    PublishDate.Date == other.PublishDate.Date &&
                    Title == other.Title
                );
    }

    public override int GetHashCode()
    {
        return PublishDate.GetHashCode();
    }

    #endregion
}
