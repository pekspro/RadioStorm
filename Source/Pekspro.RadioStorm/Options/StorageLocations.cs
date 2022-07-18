namespace Pekspro.RadioStorm.Options;

public class StorageLocations
{
    public string BaseStoragePath { get; private set; } = null!;
    
    public string LocalSettingsPath { get; private set; } = null!;
    
    public string CacheSettingsPath { get; private set; } = null!;
    
    public void ConfigureFromBasePath(string baseStoragePath)
    {        
        // Setup paths
        BaseStoragePath = baseStoragePath;
        LocalSettingsPath = Path.Combine(BaseStoragePath, "LocalState");
        CacheSettingsPath = Path.Combine(BaseStoragePath, "LocalCache");
    }

    public void Configure(string baseStoragePath, string localSettingsPath, string cacheSettingsPath)
    {
        BaseStoragePath = baseStoragePath;
        LocalSettingsPath = localSettingsPath;
        CacheSettingsPath = cacheSettingsPath;
    }
}
