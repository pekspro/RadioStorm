namespace Pekspro.RadioStorm.UI.Utilities;

public interface IWeekdaynameHelper
{
    string GetRelativeWeekdayName(DateTime day, bool allowWeekdayName = true, bool useTimeInsteadOfToday = false, bool alwaysIncludeTime = false);

    string GetWeekdayName(DateTime day);

    string GetWeekdayName(DayOfWeek dayOfWeek);
}