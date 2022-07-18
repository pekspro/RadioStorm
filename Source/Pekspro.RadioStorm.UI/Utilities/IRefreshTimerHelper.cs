namespace Pekspro.RadioStorm.UI.Utilities;

public interface IRefreshTimerHelper
{
    TimeSpan GetRefreshTimeSpan(DateTimeOffset? nextTime, bool allowQuickRefresh = false);

    void SetupTimer(IMainThreadTimer timer, DateTimeOffset? nextTime, bool allowQuickRefresh = false);
}