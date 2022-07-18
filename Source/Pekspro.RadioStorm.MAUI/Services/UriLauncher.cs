using Pekspro.RadioStorm.UI.Utilities;

namespace Pekspro.RadioStorm.MAUI.Services;

public class UriLauncher : IUriLauncher
{
    public ILogger<UriLauncher> Logger { get; }

    public UriLauncher(ILogger<UriLauncher> logger)
    {
        Logger = logger;
    }

    public async Task LaunchAsync(Uri uri)
    {
        Logger.LogInformation($"Starting URI: {uri}");

        try
        {
            await Browser.OpenAsync(uri);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed launching URI: {uri}");
        }
    }
}
