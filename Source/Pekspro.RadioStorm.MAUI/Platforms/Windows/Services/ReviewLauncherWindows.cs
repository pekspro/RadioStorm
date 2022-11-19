namespace Pekspro.RadioStorm.MAUI.Platforms.Windows.Services;

public class ReviewLauncherWindows : IReviewLauncher
{
    #region Private properties

    private IUriLauncher UriLauncher { get; }

    #endregion

    #region Constructor

    public ReviewLauncherWindows(IUriLauncher uriLauncher)
    {
        UriLauncher = uriLauncher;
    }

    #endregion

    #region Methods

    public Task LaunchReviewAsync()
    {
        return UriLauncher.LaunchAsync(new Uri($"ms-windows-store://review/?ProductId=9nblggh1nt7q"));
    }

    #endregion
}
