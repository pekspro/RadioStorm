namespace Pekspro.RadioStorm.CacheDatabase;

public class CacheDatabaseHelper
{
    #region Private properties

    private ICacheDatabaseContextFactory CacheDatabaseContextFactory { get; }

    #endregion

    #region Constructor

    public CacheDatabaseHelper(ICacheDatabaseContextFactory cacheDatabaseContextFactory)
    {
        CacheDatabaseContextFactory = cacheDatabaseContextFactory;
    }

    #endregion

    #region Methods

    public async Task MigrateAsync()
    {
        DeleteDatabase();

        using var databaseContext = CacheDatabaseContextFactory.Create();

        if (await HasPreEfDatabaseAsync().ConfigureAwait(false))
        {
            await databaseContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }

        await databaseContext.Database.MigrateAsync().ConfigureAwait(false);
    }

    public async Task<bool> HasPreEfDatabaseAsync()
    {
        try
        {
            using var databaseContext = CacheDatabaseContextFactory.Create();
            
            var res = await databaseContext.Database
                .ExecuteSqlRawAsync("SELECT * FROM __EFMigrationsHistory LIMIT 1")
                .ConfigureAwait(false);

            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    public void DeleteDatabase()
    {
        using var databaseContext = CacheDatabaseContextFactory.Create();
        
        File.Delete(databaseContext.FileName);
    }

    public bool DatabaseFileExists()
    {
        using var databaseContext = CacheDatabaseContextFactory.Create();
        
        return File.Exists(databaseContext.FileName);
    }

    #endregion
}
