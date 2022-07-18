namespace Pekspro.RadioStorm.GeneralDatabase;

public class GeneralDatabaseContextFactory : IGeneralDatabaseContextFactory
{
    public GeneralDatabaseContextFactory(IOptions<StorageLocations> storageLocationOptions)
    {
        StorageLocationOptions = storageLocationOptions;
    }

    public IOptions<StorageLocations> StorageLocationOptions { get; }

    public GeneralDatabaseContext Create()
    {
        var context = new GeneralDatabaseContext(StorageLocationOptions);
        return context;
    }
}