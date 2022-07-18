namespace Pekspro.RadioStorm.GeneralDatabase;

public class GeneralDatabaseHelper
{
    public IGeneralDatabaseContextFactory GeneralDatabaseContextFactory { get; }

    public GeneralDatabaseHelper(IGeneralDatabaseContextFactory generalDatabaseContextFactory)
    {
        GeneralDatabaseContextFactory = generalDatabaseContextFactory;
    }

    public async Task MigrateAsync()
    {
        using var GeneralDatabaseContext = GeneralDatabaseContextFactory.Create();
        
        List<DownloadState>? oldIems = null;

        if (await HasPreEfDatabaseAsync().ConfigureAwait(false))
        {
            oldIems = await TryGetCurrentDownloadStatesAsync().ConfigureAwait(false);

            await GeneralDatabaseContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }

        await GeneralDatabaseContext.Database.MigrateAsync().ConfigureAwait(false);

        if (oldIems is not null)
        {
            try
            {
                await GeneralDatabaseContext.BulkInsertAsync(oldIems);
            }
            catch (Exception )
            {

            }
        }
    }

    public async Task<bool> HasPreEfDatabaseAsync()
    {
        try
        {
            using var GeneralDatabaseContext = GeneralDatabaseContextFactory.Create();
            
            var res = await GeneralDatabaseContext.Database
                .ExecuteSqlRawAsync("SELECT * FROM __EFMigrationsHistory LIMIT 1")
                .ConfigureAwait(false);

            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    public async Task<List<DownloadState>?> TryGetCurrentDownloadStatesAsync()
    {
        try
        {
            using var databaseContext = GeneralDatabaseContextFactory.Create();
            
            List<DownloadState>? items = await databaseContext.DownloadState.AsNoTracking().ToListAsync();

            return items;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void DeleteDatabase()
    {
        using var databaseContext = GeneralDatabaseContextFactory.Create();

        File.Delete(databaseContext.FileName);
    }
    
    public bool DatabaseFileExists()
    {
        using var databaseContext = GeneralDatabaseContextFactory.Create();
        
        return File.Exists(databaseContext.FileName);
    }
}
