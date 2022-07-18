namespace Pekspro.RadioStorm.UI.Model.Program;

public partial class ProgramModel : FavoriteBaseModel
{
    #region Private properties

    private IUriLauncher UriLauncher { get; }
    
    private IEpisodesSortOrderManager EpisodesSortOrderManager { get; }

    #endregion

    #region Constructors

    internal static ProgramModel CreateWithSampleData(int sampleType = 0)
    {
        ProgramData programlData = SampleData.ProgramDataSample(sampleType);

        return new ProgramModel(null!, null!, null!, null!, programlData, null!);
    }

    public ProgramModel()
        : this(null!, null!, null!, null!, SampleData.ProgramDataSample(0), null!)
    {

    }

    public ProgramModel(
        IProgramFavoriteList programFavoriteList,
        IMessenger messenger,
        IUriLauncher uriLauncher,
        IEpisodesSortOrderManager episodesSortOrderManager,
        ProgramData programData,
        ILogger logger
        )
        : base(programFavoriteList, programData.Name ?? string.Empty, logger)
    {
        CategoryId = programData.CategoryId;
        CategoryName = programData.CategoryName ?? string.Empty;
        Description = programData.Description ?? string.Empty;
        WebPageUri = programData.ProgramUri;
        TwitterUri = programData.TwitterPageUri;
        FacebookUri = programData.FacebookPageUri;
        Id = programData.ProgramId;
        ProgramImage = new ImageLink.ImageLink(programData.ProgramImageHighResolution, programData.ProgramImageLowResolution);

        messenger?.Register<ProgramFavoriteChangedMessage>(this, (r, m) =>
        {
            if (m?.Id is null || m.Id == Id)
            {
                OnPropertyChanged(nameof(IsFavorite));
            }
        }
        );

        messenger?.Register<EpisodeSortOrderChangedMessage>(this, (r, m) =>
        {
            if (m?.Id is null || m.Id == Id)
            {
                OnPropertyChanged(nameof(SortAscending));
            }
        }
        );
        UriLauncher = uriLauncher;
        EpisodesSortOrderManager = episodesSortOrderManager;
    }

    #endregion

    #region Properties

    public string Description { get; }

    public ImageLink.ImageLink ProgramImage { get; }

    public string CategoryName { get; }

    public int CategoryId { get; }

    public string? WebPageUri { get; }

    public bool HasWebPage => !string.IsNullOrEmpty(WebPageUri);

    public string? TwitterUri { get; }

    public bool HasTwitter => !string.IsNullOrEmpty(TwitterUri);

    public string? FacebookUri { get; }

    public bool HasFacebook => !string.IsNullOrEmpty(FacebookUri);

    public bool SortAscending
    {
        get
        {
            return EpisodesSortOrderManager?.IsFavorite(Id) == true;
        }
        set
        {
            EpisodesSortOrderManager.SetFavorite(Id, value);
            OnPropertyChanged(nameof(SortAscending));
        }
    }

    public string ProgramNameAndTopEpisodeTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(EpisodeStatus?.TopEpisode?.Title))
            {
                return EpisodeStatus.TopEpisode.Title + " - " + Name;
            }                

            return Name;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgramNameAndTopEpisodeTitle))]
    private ProgramEpisodeStatusModel? _EpisodeStatus;

    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(HasWebPage))]
    private void OpenWebPage()
    {
        if (!string.IsNullOrWhiteSpace(WebPageUri))
        {
            _ = UriLauncher.LaunchAsync(WebPageUri);
        }
    }

    [RelayCommand(CanExecute = nameof(HasTwitter))]
    private void OpenTwitter()
    {
        if (!string.IsNullOrWhiteSpace(TwitterUri))
        {
            _ = UriLauncher.LaunchAsync(TwitterUri);
        }
    }

    [RelayCommand(CanExecute = nameof(HasFacebook))]
    private void OpenFacebook()
    {
        if (!string.IsNullOrWhiteSpace(FacebookUri))
        {
            _ = UriLauncher.LaunchAsync(FacebookUri);
        }
    }

    #endregion
}
