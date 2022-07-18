using Pekspro.RadioStorm.Utilities;

namespace Pekspro.RadioStorm.MAUI.Services;

public class MainThreadRunner : IMainThreadRunner
{
    public void RunInMainThread(Action action)
    {
        Application.Current.Dispatcher.Dispatch(action);
    }
}
