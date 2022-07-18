namespace Pekspro.RadioStorm.UI.Utilities;

public class RefreshTimerHelper : IRefreshTimerHelper
{
    #region Private properties

    private static readonly TimeSpan QuickMinRefreshTime = new TimeSpan(0, 0, 1);
    private static readonly TimeSpan MinRefreshTime = new TimeSpan(0, 0, 30);
    private static readonly TimeSpan MaxRefreshTime = new TimeSpan(2, 0, 0);
    private static readonly TimeSpan ExtraRefreshTime = new TimeSpan(0, 0, 5);
    private IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructor

    public RefreshTimerHelper(IDateTimeProvider dateTimeProvider)
    {
        DateTimeProvider = dateTimeProvider;
    }

    #endregion

    #region Methods
    
    public TimeSpan GetRefreshTimeSpan(DateTimeOffset? nextTime, bool allowQuickRefresh = false)
    {
        if (nextTime is null)
        {
            return MaxRefreshTime;
        }

        TimeSpan diff = nextTime.Value - DateTimeProvider.OffsetNow;

        if (allowQuickRefresh)
        {
            if (diff < QuickMinRefreshTime)
            {
                return QuickMinRefreshTime;
            }
        }
        else
        {
            if (diff < MinRefreshTime)
            {
                return MinRefreshTime;
            }
        }

        if (diff > MaxRefreshTime)
        {
            return MaxRefreshTime;
        }

        return diff;
    }

    public void SetupTimer(IMainThreadTimer timer, DateTimeOffset? nextTime, bool allowQuickRefresh = false)
    {
        TimeSpan pauseTime = GetRefreshTimeSpan(nextTime, allowQuickRefresh);

        if (!allowQuickRefresh)
        {
            pauseTime += ExtraRefreshTime;
        }

        timer.Interval = pauseTime.TotalMilliseconds;
        timer.Start();
    }

    #endregion
}
