namespace Pekspro.RadioStorm.Utilities;

public sealed class MainThreadTimer : IMainThreadTimer
{
    private bool disposedValue;

    private IMainThreadRunner MainThreadRunner { get; }

    private string Name { get; }
    
    private ILogger Logger { get; }

    private System.Timers.Timer Timer = new ();

    private Action? Callback = null;


    public MainThreadTimer(IMainThreadRunner mainThreadRunner, string name, ILogger<MainThreadTimer> logger)
    {
        MainThreadRunner = mainThreadRunner;
        Name = name;
        Logger = logger;
        Timer.Elapsed += Elapsed;
    }

    public Guid Id { get; } = Guid.NewGuid();

    private void Elapsed(object? sender, ElapsedEventArgs e)
    {
        Logger.LogTrace($"Ticking  timer {Id} {Name}");

        if (Callback is not null)
        {
            MainThreadRunner.RunInMainThread(Callback);
        }
    }

    public void SetupCallBack(Action action)
    {
        Callback = action;
    }

    public bool AutoReset
    {
        get
        {
            return Timer.AutoReset;
        }
        set
        {
            Timer.AutoReset = value;
        }
    }

    public bool Enabled
    {
        get
        {
            return Timer.Enabled;
        }
        set
        {
            Timer.Enabled = value;
        }
    }

    public double Interval
    {
        get
        {
            return Timer.Interval;
        }
        set
        {
            Timer.Interval = value;
        }
    }

    public void Start()
    {
        Logger.LogTrace($"Starting timer {Id} {Name}");

         Timer.Start();
    }

    public void Stop()
    {
        Logger.LogTrace($"Stopping timer {Id} {Name}");

        Timer.Stop();
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Timer.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
