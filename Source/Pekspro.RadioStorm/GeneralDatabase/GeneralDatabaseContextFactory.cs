namespace Pekspro.RadioStorm.GeneralDatabase;

public class GeneralDatabaseContextFactory : IGeneralDatabaseContextFactory
{
    public IOptions<StorageLocations> StorageLocationOptions { get; }
    
    public ILoggerFactory LoggerFactory { get; }

    public GeneralDatabaseContextFactory(IOptions<StorageLocations> storageLocationOptions, ILoggerFactory loggerFactory)
    {
        StorageLocationOptions = storageLocationOptions;
        LoggerFactory = loggerFactory;
    }

    public GeneralDatabaseContext Create()
    {
        var context = new GeneralDatabaseContext(StorageLocationOptions, LoggerFactory);

        return context;
    }
}