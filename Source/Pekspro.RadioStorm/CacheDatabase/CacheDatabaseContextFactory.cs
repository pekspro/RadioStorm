namespace Pekspro.RadioStorm.CacheDatabase;

public class CacheDatabaseContextFactory : ICacheDatabaseContextFactory
{
    public CacheDatabaseContextFactory(IOptions<StorageLocations> options)
    {
        Options = options;
    }

    public IOptions<StorageLocations> Options { get; }

    public CacheDatabaseContext Create()
    {
        return new CacheDatabaseContext(Options);
    }
}
