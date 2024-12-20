﻿namespace Pekspro.RadioStorm.UI.ViewModel.Settings;

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

    public string BuildTimeDetails
    {
        get
        {
            return string.Format
            (
                "{0} {1}",
                BuildInformation.BuildTime.ToShortDateString(),
                BuildInformation.BuildTime.ToShortTimeString()
            );
        }
    }

    public string ShortCommitId => BuildInformation.Git.ShortCommitId;
    
    public string DotNetVersionString => BuildInformation.DotNetSdkVersion;
    
    public string Branch => BuildInformation.Git.Branch;
    
    public string MauiVersion => BuildInformation.Workloads.MauiVersion;
    
    public ILocalSettings LocalSettings { get; }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task OpenRepository()
    {
        await UriLauncher.LaunchAsync(new Uri(Strings.General_Pekspro_Repository_Url));
    }

    #endregion
}
