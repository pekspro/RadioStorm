namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

public sealed partial class AboutViewModel
{
    #region Private properties

    private IVersionProvider VersionProvider { get; }
    private IUriLauncher UriLauncher { get; }

    #endregion

    #region Constructor

    public AboutViewModel()
    {
        VersionProvider = null!;
        UriLauncher = null!;
        LocalSettings = null!;
    }

    public AboutViewModel(IVersionProvider versionProvider, IUriLauncher uriLauncher, ILocalSettings localSettings)
    {
        VersionProvider = versionProvider;
        UriLauncher = uriLauncher;
        LocalSettings = localSettings;
    }

    #endregion

    #region Public properties

    public string VersionString => $"RadioStorm {VersionProvider.ApplicationVersion}";

    public string PureVersionString => VersionProvider.ApplicationVersion.ToString();
    
    public string BuildDetails => string.Format
        (
            Strings.About_BuildDetails, 
            BuildTimeProvider.BuildTime.ToShortDateString(),
            BuildTimeProvider.BuildTime.ToShortTimeString()
        );

    public ILocalSettings LocalSettings { get; }

    #endregion

    #region Commands

    [RelayCommand]
    private async void Email()
    {
        await UriLauncher.LaunchAsync(new Uri($"mailto:{Strings.General_Pekspro_EmailAddress}"));
    }

    [RelayCommand]
    private async void OpenWebPage()
    {
        await UriLauncher.LaunchAsync(new Uri(Strings.General_Pekspro_Url));
    }

    #endregion
}
