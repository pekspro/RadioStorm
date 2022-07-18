namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public interface IGraphHelper
{
    bool IsSignedIn { get; }
    ProviderState State { get; }
    string? UserName { get; }
    bool IsConfigured { get; }

    GraphServiceClient GetClient();
    Task InitAsync();
    Task SignIn();
    Task SignInViaCacheAsync();
    Task SignOut();
}
