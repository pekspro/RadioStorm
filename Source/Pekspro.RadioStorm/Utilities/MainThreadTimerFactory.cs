namespace Pekspro.RadioStorm.Utilities;

public sealed class MainThreadTimerFactory : IMainThreadTimerFactory
{
    private IMainThreadRunner MainThreadRunner { get; }
    public ILoggerFactory LoggerFactory { get; }

    public MainThreadTimerFactory(IMainThreadRunner mainThreadRunner, ILoggerFactory loggerFactory)
    {
        MainThreadRunner = mainThreadRunner;
        LoggerFactory = loggerFactory;
    }

    public IMainThreadTimer CreateTimer(string name)
    {
        var logger = LoggerFactory.CreateLogger<MainThreadTimer>();
        
        return new MainThreadTimer(MainThreadRunner, name, logger);
    }
}
