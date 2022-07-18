namespace Pekspro.RadioStorm.Utilities;

public interface IMainThreadRunner
{
    void RunInMainThread(Action action);
}
