namespace Pekspro.RadioStorm.UI.Utilities;

public interface IWeekdaynameHelper
{
    (string text, DateTime dateTime) GetRelativeWeekdayName(DateTime day, bool allowWeekdayName = true, bool useTimeInsteadOfToday = false, bool alwaysIncludeTime = false);
    (string text, DateTime dateTime) GetRelativeWeekdayName(DateOnly day, bool allowWeekdayName = true);
    string GetWeekdayName(DateTime day);

    string GetWeekdayName(DayOfWeek dayOfWeek);
}