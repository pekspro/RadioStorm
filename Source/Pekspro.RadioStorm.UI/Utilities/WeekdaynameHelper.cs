namespace Pekspro.RadioStorm.UI.Utilities;

public class WeekdaynameHelper : IWeekdaynameHelper
{
    #region Private properties

    private IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructor

    public WeekdaynameHelper(IDateTimeProvider dateTimeProvider)
    {
        DateTimeProvider = dateTimeProvider;
    }

    #endregion

    #region Methods

    public string GetWeekdayName(DateTime day)
    {
        return GetWeekdayName(day.DayOfWeek);
    }

    public string GetWeekdayName(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => Strings.Weekday_Monday,
        DayOfWeek.Tuesday => Strings.Weekday_Tuesday,
        DayOfWeek.Wednesday => Strings.Weekday_Wednesday,
        DayOfWeek.Thursday => Strings.Weekday_Thursday,
        DayOfWeek.Friday => Strings.Weekday_Friday,
        DayOfWeek.Saturday => Strings.Weekday_Saturday,
        _ => Strings.Weekday_Sunday,
    };

    public (string text, DateTime dateTime) GetRelativeWeekdayName(DateTime day, bool allowWeekdayName = true, bool useTimeInsteadOfToday = false, bool alwaysIncludeTime = false)
    {
        var today = DateTimeProvider.LocalNow.Date;
        int diff = (int)(day.Date - today).TotalDays;
        DateTime retDate;

        string text;

        if (diff == 0)
        {
            if (useTimeInsteadOfToday)
            {
                text = day.ToString("HH:mm");
            }
            else
            {
                text = Strings.Weekday_Today;
            }

            retDate = day.Date;
        }
        else if (diff == -1)
        {
            text = Strings.Weekday_Yesterday;
            retDate = day.Date.AddDays(-1);
        }
        else if (diff == 1)
        {
            text = Strings.Weekday_Tomorrow;
            retDate = day.Date.AddDays(1);
        }
        else if (diff >= 0 && diff < 9 && allowWeekdayName)
        {
            int maxDayCount = 0;

            if (today.AddDays(2).DayOfWeek == System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
            {
                maxDayCount = 8;
            }
            else if (today.AddDays(1).DayOfWeek == System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
            {
                maxDayCount = 7;
            }
            else
            {
                for (int i = 1; i <= 7; i++)
                {
                    if (today.AddDays(i).DayOfWeek == System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                    {
                        maxDayCount = i - 1;
                        break;
                    }
                }
            }

            if (diff <= maxDayCount)
            {
                text = GetWeekdayName(day);
            }
            else
            {
                text = day.ToString("yyyy-MM-dd");
            }

            retDate = day.Date;
        }
        else
        {
            text = day.ToString("yyyy-MM-dd");

            retDate = day.Date;
        }


        if (alwaysIncludeTime)
        {
            text += " " + day.ToString("HH:mm");

            retDate = new DateTime(retDate.Year, retDate.Month, retDate.Day, day.Hour, day.Minute, 0);
        }

        return (text, retDate);
    }

    #endregion
}
