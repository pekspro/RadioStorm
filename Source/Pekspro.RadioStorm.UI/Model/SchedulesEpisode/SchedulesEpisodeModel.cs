using TextCopy;

namespace Pekspro.RadioStorm.UI.Model.SchedulesEpisode;

public sealed partial class SchedulesEpisodeModel : ObservableObject
{
    #region Private properties

    private ILogger Logger { get; }
    
    private IWeekdaynameHelper WeekdaynameHelper { get; }
    public IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructor

    internal static SchedulesEpisodeModel CreateWithSampleData(int sampleType = 0)
    {
        var songData = SampleData.ScheduledEpisodeListItemDataSample(sampleType);
        
        var model = new SchedulesEpisodeModel(songData, null!, null!, null!);

        return model;
    }

    public SchedulesEpisodeModel()
        : this(SampleData.ScheduledEpisodeListItemDataSample(0), null!, null!, null!)
    {

    }

    public SchedulesEpisodeModel
        (
            ScheduledEpisodeListItemData data, 
            IWeekdaynameHelper weekdaynameHelper,
            IDateTimeProvider dateTimeProvider,
            ILogger<SchedulesEpisodeModel> logger
        )
    {
        Title = data.Title ?? string.Empty;
        ProgramName = data.ProgramName ?? string.Empty;
        Description = data.Description ?? string.Empty;
        ProgramId = data.ProgramId;
        Date = new DateTimeHolder.DateTimeHolder(data.Date, weekdaynameHelper);
        StartTimeUtc = data.StartTimeUtc;
        EndTimeUtc = data.EndTimeUtc;
        WeekdaynameHelper = weekdaynameHelper;
        DateTimeProvider = dateTimeProvider;
        Logger = logger;
    }

    #endregion

    #region Properties
    
    public string Title { get; }
  
    public string ProgramName { get; }
    
    public string Description { get; }
    
    public string ProgramNameOrTitle
    {
        get
        {
            if (!string.IsNullOrEmpty(ProgramName))
            {
                return ProgramName;
            }

            return Title;
        }
    }
    
    public int ProgramId { get; }
    
    public DateTimeHolder.DateTimeHolder Date { get; }
    
    public DateTimeOffset StartTimeUtc { get; set; }
    
    public DateTimeOffset EndTimeUtc { get; set; }
    
    public bool IsFinished => DateTimeProvider.OffsetNow > EndTimeUtc;

    public string RelativeStartDateString
    {
        get
        {
            (string name, _) = WeekdaynameHelper.GetRelativeWeekdayName(StartTimeUtc.LocalDateTime);

            return name;
        }
    }


    public string StartDateString => StartTimeUtc.LocalDateTime.ToString("yyyy-MM-dd");

    public string StartTimeString => StartTimeUtc.LocalDateTime.ToString("HH:mm");

    #endregion

    #region Commands

    [RelayCommand]
    private void Copy()
    {
        try
        {
            string text = $"{Title} - {ProgramName}";

            ClipboardService.SetText(text);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error copying content to Clipboard: " + ex.Message);
        }
    }

    #endregion

    #region Methods
    
    public override int GetHashCode()
    {
        return Date.GetHashCode();
    }

    #endregion
}
