namespace Pekspro.RadioStorm.UI.ViewModel.Review;

public sealed partial class ReviewViewModel : ObservableObject
{
    #region Private properties

    private ILocalSettings LocalSettingsInstance { get; }
    private IUriLauncher UriLauncher { get; }
    private ILogger Logger { get; }
    private IReviewLauncher? ReviewLauncher { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public ReviewViewModel()
    {
        LocalSettingsInstance = null!;
        UriLauncher = null!;
        Logger = null!;
    }

    public ReviewViewModel(
        ILocalSettings localSettings,
        IUriLauncher uriLauncher,
        ILogger<ReviewViewModel> logger,
        IServiceProvider serviceProvider)
    {
        LocalSettingsInstance = localSettings;
        UriLauncher = uriLauncher;
        Logger = logger;
        ReviewLauncher = serviceProvider.GetService<IReviewLauncher>();

        if (ReviewLauncher is not null && LocalSettingsInstance.MayWantToReview && LocalSettingsInstance.LaunchCount >= LocalSettings.LaunchCountBeforeAskForReview)
        {
            Logger.LogInformation("Will show review box.");
            IsVisible = true;
        }
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private bool _IsVisible;

    #endregion
    
    #region Commands

    [RelayCommand]
    private async Task Yes()
    {
        Logger.LogInformation("User wants to review.");
        
        IsVisible = false;
        if (ReviewLauncher is not null)
        {
            await ReviewLauncher.LaunchReviewAsync();
        }
        LocalSettingsInstance.MayWantToReview = false;
    }

    [RelayCommand]
    private void MaybeLater()
    {
        Logger.LogInformation("User wants to review later.");
        
        IsVisible = false;
        LocalSettingsInstance.LaunchCount = 0;
    }

    [RelayCommand]
    private void No()
    {
        Logger.LogInformation("User does not want to review.");
        
        IsVisible = false;
        LocalSettingsInstance.MayWantToReview = false;
    }

    #endregion
}
