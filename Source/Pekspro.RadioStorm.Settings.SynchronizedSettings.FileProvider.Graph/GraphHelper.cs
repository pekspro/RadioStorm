namespace Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider.Graph;

// Based on code from this repository: https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet
public class GraphHelper : IGraphHelper
{
    // UIParent used by Android version of the app
    public static object? AuthUIParent = null;

    // Keychain security group used by iOS version of the app
    public static string? iOSKeychainSecurityGroup = null;

    private PublicClientApplicationOptions appConfiguration;

    private readonly string[] Scopes = new[] { "User.Read", "Files.ReadWrite.AppFolder" };

    private IPublicClientApplication? app;

    private AuthenticationResult? AuthResult;

    private const string graphApiUrl = "https://graph.microsoft.com/v1.0/";

    private ProviderState _state = ProviderState.SignedOut;

    public string? UserName => AuthResult?.Account?.Username;

    public ProviderState State
    {
        get => _state;
        protected set
        {
            var oldState = _state;
            var newState = value;
            if (oldState != newState)
            {
                _state = newState;
                Messenger.Send(new ProviderStateChangedEventArgs(oldState, newState));
            }
        }
    }

    public GraphHelper(IMessenger messenger, 
        IOptions<PublicClientApplicationOptions> graphOptions,
        IOptions<StorageLocations> storageLocations,
        ILogger<GraphHelper> logger)
    {
        appConfiguration = graphOptions.Value;
        Messenger = messenger;
        Logger = logger;
        CacheDirectory = storageLocations.Value.LocalSettingsPath;
    }

    private string CacheDirectory { get; }
    
    public bool IsConfigured => !string.IsNullOrEmpty(Secrets.Secrets.GraphClientId);

    public async Task InitAsync()
    {
        if (!IsConfigured)
        {
            throw new Exception("Graph secrets is not configured.");
        }

        Stopwatch sp = Stopwatch.StartNew();
        Logger.LogDebug("Initializing Graph...");

        if (OperatingSystem.IsAndroid())
        {
            var builder = PublicClientApplicationBuilder
                .Create(appConfiguration.ClientId)
                .WithRedirectUri(appConfiguration.RedirectUri);

            if (!string.IsNullOrEmpty(iOSKeychainSecurityGroup))
            {
                builder = builder.WithIosKeychainSecurityGroup(iOSKeychainSecurityGroup);
            }

            app = builder.Build();
        }
        else
        {
            // Building the AAD authority, https://login.microsoftonline.com/<tenant>
            var _authority = string.Concat(appConfiguration.Instance, appConfiguration.TenantId);

            // Building a public client application
            app = PublicClientApplicationBuilder.Create(appConfiguration.ClientId)
                                                    .WithAuthority(_authority)
                                                    .WithRedirectUri(appConfiguration.RedirectUri)
                                                    .Build();

            // Building StorageCreationProperties
            var storageProperties =
                 new StorageCreationPropertiesBuilder(CacheSettings.CacheFileName, CacheDirectory)
                 .WithCacheChangedEvent(appConfiguration.ClientId)
                 .WithLinuxKeyring(
                     CacheSettings.LinuxKeyRingSchema,
                     CacheSettings.LinuxKeyRingCollection,
                     CacheSettings.LinuxKeyRingLabel,
                     CacheSettings.LinuxKeyRingAttr1,
                     CacheSettings.LinuxKeyRingAttr2)
                 .WithMacKeyChain(
                     CacheSettings.KeyChainServiceName,
                     CacheSettings.KeyChainAccountName)
                 .Build();

            

            // This hooks up the cross-platform cache into MSAL
            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
            cacheHelper.RegisterCache(app.UserTokenCache);
        }
            
        Logger.LogDebug($"Graph initialized in {sp.ElapsedMilliseconds} ms.");
    }

    public bool IsSignedIn => AuthResult is not null;

    public IMessenger Messenger { get; }
    public ILogger Logger { get; }

    public async Task SignInViaCacheAsync()
    {
        if (app is null)
        {
            return;
        }

        try
        {
            State = ProviderState.Loading;
            AuthResult = await AcquireTokenFromCache(app, Scopes).ConfigureAwait(false);
            State = AuthResult is not null ? ProviderState.SignedIn : ProviderState.SignedOut;
        }
        catch(Exception e)
        {
            Logger.LogError(e, "Failed to sign in via cache.");

            State = ProviderState.SignedOut;

            throw;
        }
    }

    public async Task SignIn()
    {
        if (app is null)
        {
            return;
        }

        State = ProviderState.Loading;
        AuthResult = await AcquireToken(app, Scopes, false).ConfigureAwait(false);
        State = AuthResult is not null ? ProviderState.SignedIn : ProviderState.SignedOut;
    }

    public async Task SignOut()
    {
        if (app is null)
        {
            return;
        }

        Logger.LogInformation("Signing out from Graph...");

        var accounts3 = await app.GetAccountsAsync().ConfigureAwait(false);
        foreach (var acc in accounts3)
        {
            await app.RemoveAsync(acc).ConfigureAwait(false);
        }

        Logger.LogInformation("Signing out from Graph completed.");

        AuthResult = null;
        State = ProviderState.SignedOut;
    }

    public async Task<GraphServiceClient> GetClientAsync()
    {
        if (app is null || AuthResult is null)
        {
            throw new RadioStormException("Not signed in into Graph/OneDrive.");
        }

        await RefreshTokenAsync().ConfigureAwait(false);

        return GetGraphServiceClient(AuthResult.AccessToken, graphApiUrl);
    }

    private async Task RefreshTokenAsync()
    {
        if (app is null || AuthResult is null)
        {
            return;
        }

        if (AuthResult.ExpiresOn < DateTimeOffset.UtcNow)
        {
            Logger.LogInformation($"Authentication has expired. Requesting new token...");
            
            var newAuth = await AcquireTokenFromCache(app, Scopes).ConfigureAwait(false);

            if (newAuth is null)
            {
                Logger.LogInformation($"Getting new authentication failed.");

                throw new RadioStormException($"Getting new authentication failed.");
            }

            AuthResult = newAuth;
        }
    }

    #region Helpers from sample app

    private async Task<AuthenticationResult> AcquireToken(IPublicClientApplication PCA, string[] scopes, bool useEmbaddedView)
    {
        AuthenticationResult result;
        try
        {
            Logger.LogInformation("Signing in into Graph. Getting accounts from cache..");

            var accounts = await PCA.GetAccountsAsync();

            Logger.LogInformation("Signing in into Graph. Acquire token from cache...");

            // Try to acquire an access token from the cache. If an interaction is required, 
            // MsalUiRequiredException will be thrown.
            result = await PCA.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();

            Logger.LogInformation("Signing in into Graph successfully.");
        }
        catch (MsalUiRequiredException ex)
        {
            Logger.LogInformation(ex, "Signing in into Graph from cache failed. Requesting user for token...");

            // Acquiring an access token interactively. MSAL will cache it so we can use AcquireTokenSilent
            // on future calls.
            if (OperatingSystem.IsAndroid())
            {
                // Prompt the user to sign-in
                var interactiveRequest = PCA.AcquireTokenInteractive(Scopes);

                if (AuthUIParent is not null)
                {
                    interactiveRequest = interactiveRequest
                        .WithParentActivityOrWindow(AuthUIParent);
                }

                result = await interactiveRequest.ExecuteAsync();

            }
            else
            {
                try
                {

                    result = await PCA.AcquireTokenInteractive(scopes)
                                // .WithUseEmbeddedWebView(false)
                                //.WithSystemWebViewOptions(GetCustomHTML())
                                .ExecuteAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Signing in failed.");

                    throw;
                }
            }

            Logger.LogInformation("Signing in into Graph via user successfully. Is valid until {expireTime}", result.ExpiresOn);
        }

        return result;
    }

    private async Task<AuthenticationResult?> AcquireTokenFromCache(IPublicClientApplication app, string[] scopes)
    {
        AuthenticationResult? result = null;
        try
        {
            Logger.LogInformation("Signing in into Graph from token in cache. Getting accounts...");

            var accounts = await app.GetAccountsAsync();

            Logger.LogInformation("Signing in into Graph from token in Cache. Acquire token...");

            // Try to acquire an access token from the cache. If an interaction is required, 
            // MsalUiRequiredException will be thrown.
            result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();

            Logger.LogInformation("Signing in into Graph from token in cache successfully. Is valid until {expireTime}", result.ExpiresOn);
        }
        catch (MsalUiRequiredException ex)
        {
            Logger.LogInformation(ex, "Signing in into Graph from token failed.");
        }

        return result;
    }

    private static GraphServiceClient GetGraphServiceClient(string accessToken, string graphApiUrl)
    {
        GraphServiceClient graphServiceClient = new GraphServiceClient(graphApiUrl,
                                                             new DelegateAuthenticationProvider(
                                                                 async (requestMessage) =>
                                                                 {
                                                                     await Task.Run(() =>
                                                                     {
                                                                         requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                                                                     });
                                                                 }));

        return graphServiceClient;
    }

    /// <summary>
    /// Returns a custom HTML for the authorization success or failure, and redirect url. 
    /// For more available options, please inspect the SystemWebViewOptions class.
    /// </summary>
    /// <returns></returns>

    private static SystemWebViewOptions GetCustomHTML()
    {
        return new SystemWebViewOptions
        {
            HtmlMessageSuccess = @"<html style='font-family: sans-serif;'>
                                      <head><title>Authentication Complete</title></head>
                                      <body style='text-align: center;'>
                                          <header>
                                              <h1>RadioStorm</h1>
                                          </header>
                                          <main style='border: 1px solid lightgrey; margin: auto; width: 600px; padding-bottom: 15px;'>
                                              <h2 style='color: limegreen;'>Authentication complete</h2>
                                              <div>You can return to the application. Feel free to close this browser tab.</div>
                                          </main>
    
                                      </body>
                                    </html>",

            HtmlMessageError = @"<html style='font-family: sans-serif;'>
                                  <head><title>Authentication Failed</title></head>
                                  <body style='text-align: center;'>
                                      <header>
                                          <h1>RadioStorm</h1>
                                      </header>
                                      <main style='border: 1px solid lightgrey; margin: auto; width: 600px; padding-bottom: 15px;'>
                                          <h2 style='color: salmon;'>Authentication failed</h2>
                                          <div><b>Error details:</b> error {0} error_description: {1}</div>
                                          <br>
                                          <div>You can return to the application. Feel free to close this browser tab.</div>
                                      </main>
    
                                  </body>
                                </html>"
        };
    }

    #endregion
}
