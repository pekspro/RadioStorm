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
// Get the Graph client id from the Azure portal, it's called Application (client) ID.
// The content of the file should be like this:

// public static partial class Secrets
// {
//    static partial void Initialize()
//    {
//        GraphClientId = "....";
//    }
// }