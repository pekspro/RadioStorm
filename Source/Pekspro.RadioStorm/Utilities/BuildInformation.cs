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

    public static string? CommitId { get; private set; }

    public static string? ShortCommitId => CommitId?.Substring(0, Math.Min(CommitId.Length, 7));

    public static string? DotNetVersionString { get; private set; }

    public static string? MauiWorkloadAndroidVersionString { get; private set; }

    public static string? MauiWorkloadWindowsVersionString { get; private set; }

    public static string? MauiWorkloadIosVersionString { get; private set; }

    public static string? MauiWorkloadMacCatalysVersionString { get; private set; }
}

/*
 * When building in GitHub, a file like this is generated setting the build time:
 
public static partial class BuildInformation
{
    static partial void Initialize()
    {
        Branch = "main";
        BuildTimeString = "2038-01-18T03:14:40Z";
        CommitId = "588e4c916b95bd3ec301662c5d68ee151b3a805c";
        DotNetVersionString = "6.0.401";
        MauiWorkloadAndroidVersionString = "6.0.541/6.0.400";
        MauiWorkloadWindowsVersionString = "6.0.541/6.0.400";
        MauiWorkloadIosVersionString = "6.0.541/6.0.400";
        MauiWorkloadMacCatalysVersionString = "6.0.541/6.0.400";
    }
}
*/
