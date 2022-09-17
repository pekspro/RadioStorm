using Pekspro.RadioStorm.Utilities;
using System;

namespace Pekspro.RadioStorm.Sandbox.Console;

public sealed class MainThreadRunner : IMainThreadRunner
{
    public void RunInMainThread(Action action)
    {
        action.Invoke();
    }
}
