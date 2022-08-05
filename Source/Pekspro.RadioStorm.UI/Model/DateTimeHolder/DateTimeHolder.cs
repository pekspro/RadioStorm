namespace Pekspro.RadioStorm.UI.Model.DateTimeHolder;

public class DateTimeHolder
{
    #region Private properties
    
    public IWeekdaynameHelper WeekdaynameHelper { get; }
    
    #endregion

    #region Constructors

    public DateTimeHolder(DateTimeOffset? startTime, IWeekdaynameHelper weekdaynameHelper)
    {
        Date = startTime;
        WeekdaynameHelper = weekdaynameHelper;
    }

    #endregion

    #region Properties

    public DateTimeOffset? Date { get; }

    public string DateString
    {
        get
        {
            if (Date is null)
            {
                return string.Empty;
            }

            return Date.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }

    public string RelativeDateString
    {
        get
        {
            if (Date is null)
            {
                return string.Empty;
            }

            (string name, _) = WeekdaynameHelper.GetRelativeWeekdayName(Date.Value.LocalDateTime, true, true, true);

            return name;
        }
    }

    #endregion
}
