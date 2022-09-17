namespace Pekspro.RadioStorm.UI.ViewModel.RecentEpisodes;

public sealed partial class RecentEpisodesViewModel : ListViewModel<RecentEpisodeModel>
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IProgramModelFactory ProgramModelFactory { get; }
    private IRecentPlayedManager RecentPlayedManager { get; }
    private IWeekdaynameHelper WeekdaynameHelper { get; }
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
        WeekdaynameHelper = null!;
        RecentPlayedManager = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        Items = new ObservableCollection<RecentEpisodeModel>();
        Items.Add(new RecentEpisodeModel(EpisodeModel.CreateWithSampleData(0), new DateTime(2022, 8, 5, 10, 11, 12), new DateOnly(2022, 8, 5), "Idag"));
        Items.Add(new RecentEpisodeModel(EpisodeModel.CreateWithSampleData(1), new DateTime(2022, 8, 5, 4, 11, 12), new DateOnly(2022, 8, 5), "Idag"));
        Items.Add(new RecentEpisodeModel(EpisodeModel.CreateWithSampleData(2), new DateTime(2022, 8, 4, 14, 16, 18), new DateOnly(2022, 8, 4), "Igår"));
    }

    public RecentEpisodesViewModel(
        IDataFetcher dataFetcher,
        IEpisodeModelFactory episodeModelFactory,
        IProgramModelFactory programModelFactory,
        IRecentPlayedManager recentPlayedManager,
        IWeekdaynameHelper weekdaynameHelper,  
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
        WeekdaynameHelper = weekdaynameHelper;
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

        List<KeyValuePair<RecentEpisodeModel, DateTimeOffset>> pendingItemsToAdd = 
            new List<KeyValuePair<RecentEpisodeModel, DateTimeOffset>>();
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
                (string dayName, DateTime date) = WeekdaynameHelper.GetRelativeWeekdayName(recentItem.Timestamp.DateTime);

                RecentEpisodeModel recentEpisodeModel = new RecentEpisodeModel(episodeModel,
                    recentItem.Timestamp.DateTime,
                    DateOnly.FromDateTime(date),
                    dayName);

                pendingItemsToAdd.Add(new KeyValuePair<RecentEpisodeModel, DateTimeOffset>(recentEpisodeModel, recentItem.Timestamp));
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
                if (item.Model.ProgramId.HasValue)
                {
                    var programData = await DataFetcher.GetProgramAsync(item.Model.ProgramId.Value, refreshSettings.AllowCache, cancellationToken);

                    if (programData is not null)
                    {
                        item.Model.ProgramDetails = ProgramModelFactory.Create(programData);
                    }
                }
            }
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }
    }

    protected override int GetId(RecentEpisodeModel item) => item.Model.Id;

    protected override int Compare(RecentEpisodeModel a1, RecentEpisodeModel b1)
    {
        int timeCompare = a1.LatestListenTime.CompareTo(b1.LatestListenTime);
        if (timeCompare != 0)
        {
            return timeCompare;
        }

        var a = a1.Model;
        var b = b1.Model;

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

    protected override int GetGroupPriority(RecentEpisodeModel item)
    {
        return -(item.DatePeriod.Year * 10000 + item.DatePeriod.Month * 100 + item.DatePeriod.Day);
    }

    protected override string GetGroupName(RecentEpisodeModel item) => item.RecentPeriod ?? "?";

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
