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

    public string? BuildTimeDetails
    {
        get
        {
            if (BuildInformation.BuildTimeString is null)
            {
                return null;
            }
            
            return string.Format
            (
                "{0} {1}",
                BuildInformation.BuildTime.ToShortDateString(),
                BuildInformation.BuildTime.ToShortTimeString()
            );
        }
    }

    public string? ShortCommitId => BuildInformation.ShortCommitId;
    
    public string? DotNetVersionString => BuildInformation.DotNetVersionString;
    
    public string? Branch => BuildInformation.Branch;
    
    public string? MauiWorkloadWindowsVersionString => BuildInformation.MauiWorkloadWindowsVersionString;
    
    public string? MauiWorkloadAndroidVersionString => BuildInformation.MauiWorkloadAndroidVersionString;
    
    public string? MauiWorkloadIosVersionString => BuildInformation.MauiWorkloadIosVersionString;
    
    public string? MauiWorkloadMacCatalysVersionString => BuildInformation.MauiWorkloadMacCatalysVersionString;

    public ILocalSettings LocalSettings { get; }

    #endregion

    #region Commands

    [RelayCommand]
    private async void OpenRepository()
    {
        await UriLauncher.LaunchAsync(new Uri(Strings.General_Pekspro_Repository_Url));
    }

    #endregion
}
