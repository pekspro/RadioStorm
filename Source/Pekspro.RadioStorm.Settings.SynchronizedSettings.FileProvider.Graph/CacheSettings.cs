namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public static class CacheSettings
{
    // computing the root directory is not very simple on Linux and Mac, so a helper is provided
    public static readonly string CacheFileName = "msalcache.dat";

    public static readonly string KeyChainServiceName = "Pekspro.RadioStorm";
    public static readonly string KeyChainAccountName = "MSALCache";

    public static readonly string LinuxKeyRingSchema = "com.pekspro.radiostorm.tokencache";
    public static readonly string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
    public static readonly string LinuxKeyRingLabel = "MSAL token cache for PEK's Productions RadioStorm.";
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
    public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "RadioStorm");
}
