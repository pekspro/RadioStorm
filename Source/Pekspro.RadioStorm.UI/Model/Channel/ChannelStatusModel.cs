namespace Pekspro.RadioStorm.UI.Model.Channel;

public class ChannelStatusModel : ObservableObject
{
    #region Private properties
    public IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructors

    public ChannelStatusModel(ChannelStatusData d, IDateTimeProvider dateTimeProvider, ImageLink.ImageLink? channelImageUrl)
        : this(d, dateTimeProvider, channelImageUrl?.HighResolution, channelImageUrl?.LowResolution)
    {
    }

    public ChannelStatusModel(ChannelStatusData d, IDateTimeProvider dateTimeProvider, string? channelHighResolutionImageUrl, string? channelLowResolutionImageUrl)
    {
        DateTimeProvider = dateTimeProvider;

        if (d.CurrentProgramId != 0)
        {
            CurrentProgramId = d.CurrentProgramId;
        }
        else
        {
            CurrentProgramId = null;
        }

        CurrentProgram = d.CurrentProgram;
        CurrentProgramImage = new ImageLink.ImageLink(null, d.CurrentProgramImage, channelHighResolutionImageUrl, channelLowResolutionImageUrl);
        CurrentProgramDescription = d.CurrentProgramDescription;
        CurrentTimePeriod = new TimePeriod.TimePeriod(d.CurrentStartTime, d.CurrentEndTime);

        if (d.NextProgramId != 0)
        {
            NextProgramId = d.NextProgramId;
        }
        else
        {
            NextProgramId = null;
        }

        NextProgram = d.NextProgram;
        NextProgramImage = new ImageLink.ImageLink(null, d.NextProgramImage, channelHighResolutionImageUrl, channelLowResolutionImageUrl);
        NextProgramDescription = d.NextProgramDescription;
        NextTimePeriod = new TimePeriod.TimePeriod(d.NextStartTime, d.NextEndTime);
    }

    #endregion

    #region Properties

    public int? CurrentProgramId { get; }

    public string? CurrentProgram { get; }

    public string? CurrentProgramDescription { get; }

    public ImageLink.ImageLink? CurrentProgramImage { get; }
    
    public TimePeriod.TimePeriod? CurrentTimePeriod { get; }

    public int? NextProgramId { get; }

    public string? NextProgram { get; }

    public string? NextProgramDescription { get; }

    public ImageLink.ImageLink? NextProgramImage { get; }

    public TimePeriod.TimePeriod? NextTimePeriod { get; }


    public double CurrentProgressPosition
    {
        get
        {
            var current = CurrentTimePeriod;
            if (current is null || current.StartTime is null || current.EndTime is null ||
                current.StartTime >= current.EndTime
                )
            {
                return 0;
            }

            var currentTime = DateTimeProvider.OffsetNow;

            if (current.StartTime > currentTime)
            {
                return 0;
            }

            if (current.EndTime < currentTime)
            {
                return (current.EndTime - current.StartTime).Value.TotalSeconds;
            }

            return (currentTime - current.StartTime).Value.TotalSeconds;
        }
    }

    public double CurrentProgressLength
    {
        get
        {
            var current = CurrentTimePeriod;
            if (current is null || current.StartTime is null || current.EndTime is null ||
                current.StartTime >= current.EndTime
                )
            {
                return 1;
            }

            return (current.EndTime - current.StartTime).Value.TotalSeconds;
        }
    }

    public float CurrentProgressRatio
    {
        get
        {
            double currentLength = CurrentProgressLength;

            if (currentLength <= 0)
            {
                return 0;
            }

            double currentPos = CurrentProgressPosition;

            return (float)(currentPos / currentLength);                
        }
    }

    public DateTimeOffset? NextRefreshTime
    {
        get
        {
            if (NextTimePeriod?.StartTime is not null)
            {
                if (CurrentTimePeriod?.EndTime is not null && CurrentTimePeriod?.EndTime < NextTimePeriod.StartTime)
                {
                    return CurrentTimePeriod?.EndTime;
                }

                return NextTimePeriod?.StartTime;
            }

            return CurrentTimePeriod?.EndTime;
        }
    }

    #endregion

    #region Methods

    internal void NotifyCurrentProgressChanged()
    {
        OnPropertyChanged(nameof(CurrentProgressPosition));
        OnPropertyChanged(nameof(CurrentProgressLength));
        OnPropertyChanged(nameof(CurrentProgressRatio));
    }

    #endregion
}
