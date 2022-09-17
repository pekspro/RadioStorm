namespace Pekspro.RadioStorm.UI.ViewModel.Episode;

public sealed partial class DownloadsViewModel : ListViewModel<EpisodeModel>
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IProgramModelFactory ProgramModelFactory { get; }
    private IDownloadManager DownloadManager { get; }
    private IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public DownloadsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        EpisodeModelFactory = null!;
        ProgramModelFactory = null!;
        DownloadManager = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        Items = new ObservableCollection<EpisodeModel>();
        Items.Add(EpisodeModel.CreateWithSampleData(0));
        Items.Add(EpisodeModel.CreateWithSampleData(1));
        Items.Add(EpisodeModel.CreateWithSampleData(2));
    }

    public DownloadsViewModel(
        IDataFetcher dataFetcher,
        IEpisodeModelFactory episodeModelFactory,
        IProgramModelFactory programModelFactory,
        IDownloadManager downloadManager,
        IMessenger messenger,
        IDateTimeProvider dateTimeProvider,
        IMainThreadRunner mainThreadRunner,
        ILogger<RecentEpisodesViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        EpisodeModelFactory = episodeModelFactory;
        ProgramModelFactory = programModelFactory;
        DownloadManager = downloadManager;
        DateTimeProvider = dateTimeProvider;
        messenger.Register<DownloadDeleted>(this, (r, m) =>
        {
            QueueRefresh(new RefreshSettings());
        }
        );

        messenger.Register<DownloadAdded>(this, (r, m) =>
        {
            QueueRefresh(new RefreshSettings());
        }
        );
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private bool _HasDownloadItems;

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

        var items = DownloadManager.GetDownloads();
        var recentItems = items.OrderBy(a => a.ProgramId).ThenBy(a => a.EpisodeId).ToList();

        HasDownloadItems = recentItems.Any();

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

        DeleteObselete(recentItems.Select(a => a.EpisodeId).ToList());

        for (int i = 0; i < recentItems.Count; i++)
        {
            var recentItem = recentItems[i];

            var id = recentItem.EpisodeId;

            var dbEpisodeData = await DataFetcher.GetEpisodeAsync(id, refreshSettings.AllowCache, cancellationToken);
            
            if (dbEpisodeData is not null)
            {
                EpisodeModel episodeModel = EpisodeModelFactory.Create(dbEpisodeData);

                pendingItemsToAdd.Add(new KeyValuePair<EpisodeModel, DateTimeOffset>(episodeModel, DateTimeOffset.UtcNow));
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
    
    #endregion
}
