namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

public interface IGraphHelper
{
    bool IsSignedIn { get; }
    ProviderState State { get; }
    string? UserName { get; }
    bool IsConfigured { get; }

    Task<GraphServiceClient> GetClientAsync();
    Task InitAsync();
    Task SignIn();
    Task SignInViaCacheAsync();
    Task SignOut();
}
