namespace Pekspro.RadioStorm.Utilities;

public static partial class BuildInformation
{
    static BuildInformation()
    {
        Initialize();
    }

    static partial void Initialize();

    public static string? Branch { get; private set; }

    public static string? BuildTimeString { get; private set; }

    public static DateTime BuildTime =>
        BuildTimeString is null ? DateTime.MinValue : DateTime.Parse(BuildTimeString);

    public static string? DotNetVersionString { get; private set; }

    public static string? MauiWorkloadAndroidVersionString { get; private set; }

    public static string? MauiWorkloadWindowsVersionString { get; private set; }

    public static string? MauiWorkloadIosVersionString { get; private set; }

    public static string? MauiWorkloadMacCatalysVersionString { get; private set; }
}

/*
 * When building in GitHub, a file like this is generated setting the build time:
 
public static sealed partial class BuildInformation
{
    static partial void Initialize()
    {
        Branch = "main";
        BuildTimeString = "2022-07-16T18:41:12Z";
        DotNetVersionString = "2022-07-16T18:41:12Z";
        MauiWorkloadAndroidVersionString = "6.0.486/6.0.400";
        MauiWorkloadWindowsVersionString = "6.0.486/6.0.400";
        MauiWorkloadIosVersionString = "6.0.486/6.0.400";
        MauiWorkloadMacCatalysVersionString = "6.0.486/6.0.400";
    }
}
*/
