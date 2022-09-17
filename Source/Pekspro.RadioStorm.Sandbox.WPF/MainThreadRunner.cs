namespace Pekspro.RadioStorm.Sandbox.WPF;

public sealed class MainThreadRunner : IMainThreadRunner
{
    public void RunInMainThread(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
}
