namespace Pekspro.RadioStorm.Options;

public class StorageLocations
{
    public string BaseStoragePath { get; private set; } = null!;
    
    public string LocalSettingsPath { get; private set; } = null!;
    
    public string CacheSettingsPath { get; private set; } = null!;
    
    public string TemporaryPath { get; private set; } = null!;
    
    public void ConfigureFromBasePath(string baseStoragePath, string? cachePath = null, string? temporaryPath = null)
    {        
        // Setup paths
        BaseStoragePath = baseStoragePath;
        LocalSettingsPath = Path.Combine(BaseStoragePath, "LocalState");
        CacheSettingsPath = cachePath ?? Path.Combine(BaseStoragePath, "LocalCache");
        TemporaryPath = temporaryPath ?? Path.Combine(BaseStoragePath, "Temp");
    }

    public void Configure(string baseStoragePath, string localSettingsPath, string cacheSettingsPath, string temporaryPath)
    {
        BaseStoragePath = baseStoragePath;
        LocalSettingsPath = localSettingsPath;
        CacheSettingsPath = cacheSettingsPath;
        TemporaryPath = temporaryPath;
    }
}
