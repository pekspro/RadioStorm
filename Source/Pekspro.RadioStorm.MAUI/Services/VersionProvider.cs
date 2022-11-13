namespace Pekspro.RadioStorm.MAUI.Services;

public sealed class VersionProvider : IVersionProvider
{
    static VersionProvider()
    {
        string version = AppInfo.VersionString;

        CurrentVersion = new Version(version);
    }

    public static Version CurrentVersion;

    public Version ApplicationVersion => CurrentVersion;
}
