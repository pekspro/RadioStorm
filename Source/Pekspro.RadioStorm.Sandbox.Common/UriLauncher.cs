using Pekspro.RadioStorm.UI.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pekspro.RadioStorm.Sandbox.Common;

public sealed class UriLauncher : IUriLauncher
{
    public ILogger<UriLauncher> Logger { get; }

    public UriLauncher(ILogger<UriLauncher> logger)
    {
        Logger = logger;
    }

    public Task LaunchAsync(Uri uri)
    {
        Logger.LogInformation($"Starting URI: {uri}");

        string url = uri.ToString();

        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }

        return Task.CompletedTask;
    }
}
