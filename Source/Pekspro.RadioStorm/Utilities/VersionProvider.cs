namespace Pekspro.RadioStorm.Utilities;

public sealed class VersionProvider : IVersionProvider
{
    static VersionProvider()
    {
        CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
    }

    public static Version CurrentVersion;

    public Version ApplicationVersion => CurrentVersion;
}
