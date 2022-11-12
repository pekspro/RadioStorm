using Android.Content.PM;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

public sealed class AndroidVersionProvider : IVersionProvider
{
    static AndroidVersionProvider()
    {
        var context = MainApplication.Context;
        PackageManager manager = context.PackageManager!;
        PackageInfo info;

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            info = manager.GetPackageInfo(context.PackageName!, PackageManager.PackageInfoFlags.Of((int) PackageInfoFlags.MetaData));
        }
        else
        {
            #pragma warning disable 618
            info = manager.GetPackageInfo(context.PackageName!, PackageInfoFlags.MetaData)!;
            #pragma warning restore 618
        }

        string version = info.VersionName!;

        CurrentVersion = new Version(version);
    }

    public static Version CurrentVersion;

    public Version ApplicationVersion => CurrentVersion;
}
