namespace Pekspro.RadioStorm.UI.Utilities;

public interface IUriLauncher
{
    public Task LaunchAsync(string uri)
    {
        return LaunchAsync(new Uri(uri));
    }

    public Task LaunchAsync(Uri uri);
}
