namespace Pekspro.RadioStorm.MAUI.Utilities;

/// <summary>
/// Helps with setting background color when swiping.
/// 
/// We don't want to always have a background color. It's better with
/// transparent to get an hovering effect on Windows. But transparent
/// is no good when swiping.
/// </summary>
public static class SwipeHelper
{
    public static void SwipeStarted(object sender)
    {
        if (sender is SwipeView swipeView)
        {
            if (Application.Current!.RequestedTheme == AppTheme.Dark)
            {
                swipeView.Content.BackgroundColor = Colors.Black;
            }
            else
            {
                swipeView.Content.BackgroundColor = Colors.White;
            }
        }
    }

    public static async void SwipeEnded(object sender)
    {
        if (sender is SwipeView swipeView)
        {
            await Task.Delay(200);
            swipeView.Content.BackgroundColor = null;
        }
    }
}
