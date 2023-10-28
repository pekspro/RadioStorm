namespace Pekspro.RadioStorm.MAUI.Services;

public sealed class MainThreadRunner : IMainThreadRunner
{
    public void RunInMainThread(Action action)
    {
        Application.Current!.Dispatcher.Dispatch(action);
    }
}
