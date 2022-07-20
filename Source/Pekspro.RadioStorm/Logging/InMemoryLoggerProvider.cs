namespace Pekspro.RadioStorm.Logging;

public class InMemLoggerProvider : ILoggerProvider
{
    private readonly InMemoryLogger logger;

    public InMemLoggerProvider(InMemoryLogger logger) => this.logger = logger;

    public ILogger CreateLogger(string categoryName) => logger;

    public void Dispose() { }
}
