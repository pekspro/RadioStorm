namespace Pekspro.RadioStorm.UI.Model.PublishLength;

public sealed partial class PublishLength : ObservableObject
{
    #region Constructor

    public PublishLength(IWeekdaynameHelper weekdaynameHelper)
        : this(null, weekdaynameHelper)
    {

    }

    public PublishLength(DateTimeOffset? startTime, IWeekdaynameHelper weekdaynameHelper)
        : this(startTime, 0, weekdaynameHelper)
    {
    }
    public PublishLength(DateTimeOffset? startTime, int durationInSeconds, IWeekdaynameHelper weekdaynameHelper)
        : this(startTime, new TimeSpan(0, 0, durationInSeconds), weekdaynameHelper)
    {
    }

    public PublishLength(DateTimeOffset? startTime, TimeSpan length, IWeekdaynameHelper weekdaynameHelper)
    {
        PublishDate = startTime;
        WeekdaynameHelper = weekdaynameHelper;
        Length = length;
    }

    #endregion

    #region Properties

    public DateTimeOffset? PublishDate { get; }
    public IWeekdaynameHelper WeekdaynameHelper { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LengthString))]
    private TimeSpan _Length;

    public string PublishDateString
    {
        get
        {
            if (PublishDate is null)
            {
                return string.Empty;
            }

            return PublishDate.Value.LocalDateTime.ToString("yyyy-MM-dd");
        }
    }

    public string PublishDateMonth
    {
        get
        {
            if (PublishDate is null)
            {
                return string.Empty;
            }

            return PublishDate.Value.LocalDateTime.ToString("yyyy-MM");
        }
    }

    public string RelativePublishDateString
    {
        get
        {
            if (PublishDate is null)
            {
                return string.Empty;
            }

            (string name, _) = WeekdaynameHelper.GetRelativeWeekdayName(PublishDate.Value.LocalDateTime, true, true);
            
            return name;
        }
    }

    public string LengthString
    {
        get
        {
            if (Length.TotalSeconds <= 0)
            {
                return string.Empty;
            }

            return Length.ToString("hh\\:mm");
        }
    }

    #endregion
}
