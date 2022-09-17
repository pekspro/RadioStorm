namespace Pekspro.RadioStorm.CacheDatabase;

public sealed class CacheDatabaseContextFactory : ICacheDatabaseContextFactory
{
    public IOptions<StorageLocations> Options { get; }

    public ILoggerFactory LoggerFactory { get; }

    public CacheDatabaseContextFactory(IOptions<StorageLocations> options, ILoggerFactory loggerFactory)
    {
        Options = options;
        LoggerFactory = loggerFactory;
    }

    public CacheDatabaseContext Create()
    {
        return new CacheDatabaseContext(Options, LoggerFactory);
    }
}
