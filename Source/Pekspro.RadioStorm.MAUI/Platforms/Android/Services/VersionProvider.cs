using Android.Content.PM;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

public class AndroidVersionProvider : IVersionProvider
{
    static AndroidVersionProvider()
    {
        var context = MainApplication.Context;
        PackageManager manager = context.PackageManager!;
        PackageInfo info = manager.GetPackageInfo(context.PackageName!, 0)!;
        string version = info.VersionName!;

        CurrentVersion = new Version(version);
    }

    public static Version CurrentVersion;

    public Version ApplicationVersion => CurrentVersion;
}
