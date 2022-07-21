using Windows.ApplicationModel;

namespace Pekspro.RadioStorm.MAUI.Platforms.Windows.Services;

public class WindowsVersionProvider : IVersionProvider
{
    static WindowsVersionProvider()
    {
        Package package = Package.Current;
        PackageId packageId = package.Id;
        PackageVersion version = packageId.Version;

        CurrentVersion = new Version(version.Major, version.Minor, version.Build, version.Revision);
    }

    public static Version CurrentVersion;

    public Version ApplicationVersion => CurrentVersion;
}
