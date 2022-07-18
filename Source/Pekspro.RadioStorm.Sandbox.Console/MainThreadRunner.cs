using Pekspro.RadioStorm.Utilities;
using System;

namespace Pekspro.RadioStorm.Sandbox.Console;

public class MainThreadRunner : IMainThreadRunner
{
    public void RunInMainThread(Action action)
    {
        action.Invoke();
    }
}
