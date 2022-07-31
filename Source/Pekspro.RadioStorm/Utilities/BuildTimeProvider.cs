namespace Pekspro.RadioStorm.Utilities;

public static partial class BuildTimeProvider
{
    static BuildTimeProvider()
    {
        Initialize();
    }

    static partial void Initialize();

    public static string? BuildTimeString { get; private set; }

    public static DateTime BuildTime =>
        BuildTimeString is null ? DateTime.MinValue : DateTime.Parse(BuildTimeString);
}

/*
 * When building in GitHub, a file like this is generated setting the build time:
 
public static partial class BuildTimeProvider
{
    static partial void Initialize()
    {
        BuildTimeString = "2022-07-16T18:41:12Z";
    }
}
*/