namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Secrets;

public static partial class Secrets
{
    static Secrets()
    {
        Initialize();
    }

    static partial void Initialize();

    public static string? GraphClientId { get; private set; }

    public static bool IsTestEnvironment { get; private set; } = true;

}

// To assign secrets, create a file named Secrets-Protected.cs.
// This file is ignored in git.
// The content of the file should be like this:

// public static sealed partial class Secrets
// {
//    static partial void Initialize()
//    {
//        GraphClientId = "....";
//    }
// }