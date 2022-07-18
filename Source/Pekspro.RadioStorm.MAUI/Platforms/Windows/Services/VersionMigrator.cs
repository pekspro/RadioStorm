namespace Pekspro.RadioStorm.MAUI.Platforms.Windows.Services;
using global::Windows.Storage;

internal class VersionMigrator : IVersionMigrator
{
    public VersionMigrator(ILogger<VersionMigrator> logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }

    public async Task MigrateAsync(string previousVersion, string newVersion)
    {
        string[] filenamesToDelete = new string[] { "basicProgramData.dat", "RadioStormCacheFile", "RadioStormProgramDataCacheFile" };

        foreach (string f in filenamesToDelete)
        {
            try
            {
                var file = (await ApplicationData.Current.TemporaryFolder.CreateFileAsync(f, CreationCollisionOption.OpenIfExists));
                if (file is not null)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception)
            {
            }
        }

        Logger.LogInformation($"Version is changed. Moving download folder");

        try
        {

            var oldFolder =
                await ApplicationData.Current.LocalFolder.GetFolderAsync("downloads");

            var newFolder =
                await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("downloads", CreationCollisionOption.OpenIfExists);

            foreach (var file in await oldFolder.GetFilesAsync())
            {
                try
                {
                    await file.MoveAsync(newFolder);
                }
                catch (Exception)
                {
                }
            }

            await oldFolder.DeleteAsync();
        }
        catch (Exception)
        {

        }
    }
}
