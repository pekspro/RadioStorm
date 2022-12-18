using Npgsql.Logging;

namespace Pekspro.RadioStorm.UI.Model.DateTimeHolder;

public sealed class DateTimeHolder
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

    private string? RelativeDateAndTimeStringCache;

    public string RelativeDateAndTimeString
    {
        get
        {
            if (RelativeDateAndTimeStringCache is null)
            {
                if (Date is null)
                {
                    RelativeDateAndTimeStringCache = string.Empty;
                }
                else
                {
                    (RelativeDateAndTimeStringCache, _) = WeekdaynameHelper.GetRelativeWeekdayName(Date.Value.LocalDateTime, true, false, true);
                }
            }

            return RelativeDateAndTimeStringCache;
        }
    }

    private string? RelativeDateNameCache;

    public string RelativeDateName
    {
        get
        {
            if (RelativeDateNameCache is null)
            {
                if (Date is null)
                {
                    RelativeDateNameCache = string.Empty;
                }
                else
                {
                    (RelativeDateNameCache, _) = WeekdaynameHelper.GetRelativeWeekdayName(Date.Value.LocalDateTime, true, false, false);
                }
            }

            return RelativeDateNameCache;
        }
    }

    #endregion
}
