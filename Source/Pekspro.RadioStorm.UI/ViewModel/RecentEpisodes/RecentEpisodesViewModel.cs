namespace Pekspro.RadioStorm.UI.ViewModel.RecentEpisodes;

public partial class RecentEpisodesViewModel : ListViewModel<EpisodeModel>
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IProgramModelFactory ProgramModelFactory { get; }
    private IRecentPlayedManager RecentPlayedManager { get; }
    private IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public RecentEpisodesViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        EpisodeModelFactory = null!;
        ProgramModelFactory = null!;
        RecentPlayedManager = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        Items.Add(EpisodeModel.CreateWithSampleData(0));
        Items.Add(EpisodeModel.CreateWithSampleData(1));
        Items.Add(EpisodeModel.CreateWithSampleData(2));
    }

    public RecentEpisodesViewModel(
        IDataFetcher dataFetcher,
        IEpisodeModelFactory episodeModelFactory,
        IProgramModelFactory programModelFactory,
        IRecentPlayedManager recentPlayedManager,
        IMessenger messenger,
        IDateTimeProvider dateTimeProvider,
        IMainThreadRunner mainThreadRunner,
        ILogger<RecentEpisodesViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        EpisodeModelFactory = episodeModelFactory;
        ProgramModelFactory = programModelFactory;
        RecentPlayedManager = recentPlayedManager;
        DateTimeProvider = dateTimeProvider;
        messenger.Register<RecentListChangedMessage>(this, (r, m) =>
        {
            QueueRefresh(new RefreshSettings());
        }
        );
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private bool _HasRecentItems;

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        if (refreshSettings.FullRefresh)
        {
            ClearLists();
        }
        
        if (Items is null || !Items.Any())
        {
            DownloadState = DownloadStates.Downloading;
        }

        var recentItems = RecentPlayedManager.Items.Values.Where(o => o.IsRemoved == false && o.IsEpisode).OrderByDescending(a => a.Timestamp).ToList();

        HasRecentItems = recentItems.Any();

        List<KeyValuePair<EpisodeModel, DateTimeOffset>> pendingItemsToAdd = new List<KeyValuePair<EpisodeModel, DateTimeOffset>>();
        DateTime latestAddTime = DateTimeProvider.UtcNow;
        
        void AddFetchedItems()
        {
            foreach (var item in pendingItemsToAdd)
            {
                Insert(item.Key);

                DownloadState = DownloadStates.Done;
            }

            pendingItemsToAdd.Clear();
        }

        DeleteObselete(recentItems.Select(a => a.Id).ToList());

        for (int i = 0; i < recentItems.Count; i++)
        {
            var recentItem = recentItems[i];

            var id = recentItem.Id;

            var dbEpisodeData = await DataFetcher.GetEpisodeAsync(id, refreshSettings.AllowCache, cancellationToken);

            if (dbEpisodeData is not null)
            {
                EpisodeModel episodeModel = EpisodeModelFactory.Create(dbEpisodeData);

                pendingItemsToAdd.Add(new KeyValuePair<EpisodeModel, DateTimeOffset>(episodeModel, recentItem.Timestamp));
            }

            DateTime now = DateTimeProvider.UtcNow;
            if (i >= recentItems.Count - 1 || (now - latestAddTime).TotalMilliseconds >= 1000)
            {
                latestAddTime = now;
                AddFetchedItems();
            }
        }

        AddFetchedItems();

        if (Items?.Count > 0)
        {
            DownloadState = DownloadStates.Done;

            foreach (var item in Items.ToArray())
            {
                if (item.ProgramId.HasValue)
                {
                    var programData = await DataFetcher.GetProgramAsync(item.ProgramId.Value, refreshSettings.AllowCache, cancellationToken);

                    if (programData is not null)
                    {
                        item.ProgramDetails = ProgramModelFactory.Create(programData);
                    }
                }
            }
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }

    }

    protected override int GetId(EpisodeModel item) => item.Id;

    protected override int Compare(EpisodeModel a, EpisodeModel b)
    {
        int nameCompare = a.ProgramName.CompareTo(b.ProgramName);

        if (nameCompare != 0)
        {
            return nameCompare;
        }

        if (a.PublishLength.PublishDate is null)
        {
            return 1;
        }

        if (b.PublishLength.PublishDate is null)
        {
            return -1;
        }

        return a.PublishLength.PublishDate.Value.CompareTo(b.PublishLength.PublishDate.Value);
    }

    protected override string GetGroupName(EpisodeModel item) => item.ProgramName ?? "?";

    public void RemoveFromRecentList(EpisodeModel episodeModel)
    {
        RecentPlayedManager.Remove(true, episodeModel.Id);
    }

    public void RemoveFromRecentList(EpisodeModel[] episodeModels)
    {
        foreach (var episodeModel in episodeModels)
        {
            RecentPlayedManager.Remove(true, episodeModel.Id);
        }
    }

    #endregion
}
