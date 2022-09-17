namespace Pekspro.RadioStorm.UI.Model.TimePeriod;

public sealed class TimePeriod
{
    #region Constructor

    public TimePeriod()
    {

    }

    public TimePeriod(DateTimeOffset? startTime, DateTimeOffset? endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    #endregion

    #region Properites

    public DateTimeOffset? StartTime { get; }

    public DateTimeOffset? EndTime { get; }

    public TimeSpan Length
    {
        get
        {
            if (StartTime is null || EndTime is null || EndTime < StartTime)
            {
                return TimeSpan.Zero;
            }

            return EndTime.Value - StartTime.Value;
        }
    }

    public string StartTimeString
    {
        get
        {
            if (StartTime is null)
            {
                return string.Empty;
            }

            return StartTime.Value.LocalDateTime.ToString("HH:mm");
        }
    }

    public string EndTimeString
    {
        get
        {
            if (EndTime is null)
            {
                return string.Empty;
            }

            return EndTime.Value.LocalDateTime.ToString("HH:mm");
        }
    }

    public string PeriodTimeString
    {
        get
        {
            string startString = StartTimeString;
            string endString = EndTimeString;

            if (string.IsNullOrEmpty(startString))
            {
                return endString;
            }

            if (string.IsNullOrEmpty(endString))
            {
                return startString;
            }

            return $"{startString}-{endString}";
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
