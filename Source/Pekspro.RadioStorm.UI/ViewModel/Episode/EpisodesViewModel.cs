namespace Pekspro.RadioStorm.UI.ViewModel.Episode;

public partial class EpisodesViewModel : ListViewModel<EpisodeModel>, ISearch
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IEpisodesSortOrderManager EpisodesSortOrderManager { get; }
    public IListenStateManager ListenStateManager { get; }
    private IAudioManager AudioManager { get; }
    
    public const int MaxEpisodeDownloadCount = 200;

    private bool DownloadAllEpisodesOnRefresh = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public EpisodesViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        EpisodeModelFactory = null!;
        EpisodesSortOrderManager = null!;
        AudioManager = null!;
        ListenStateManager = null!;
        DownloadState = DownloadStates.Done;

        Items = new ObservableCollection<EpisodeModel>();
        Items.Add(EpisodeModel.CreateWithSampleData(0));
        Items.Add(EpisodeModel.CreateWithSampleData(1));
        Items.Add(EpisodeModel.CreateWithSampleData(2));
    }

    public EpisodesViewModel(
        IDataFetcher dataFetcher,
        IEpisodeModelFactory programModelFactory,
        IEpisodesSortOrderManager episodesSortOrderManager,
        IListenStateManager listenStateManager,
        IAudioManager audioManager,
        IMessenger messenger,
        IMainThreadRunner mainThreadRunner,
        ILogger<EpisodesViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        EpisodeModelFactory = programModelFactory;
        EpisodesSortOrderManager = episodesSortOrderManager;
        ListenStateManager = listenStateManager;
        AudioManager = audioManager;
        messenger?.Register<EpisodeSortOrderChangedMessage>(this, (r, m) =>
        {
            if (m?.Id is null || m.Id == ProgramId)
            {
                ClearLists();
                QueueRefresh(new RefreshSettings());
            }
        }
        );

        messenger?.Register<ListenStateChangedMessage>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(FirstNotListenedEpisodePosition));
        }
        );
    }

    #endregion

    #region Properties

    public int ProgramId { get; internal set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadAllEpisodesCommand))]
    private bool _HasMoreEpisodes;

    private bool CanAutoCreatePlayList
    {
        get
        {
            if (DownloadState != DownloadStates.Done)
            {
                return false;
            }

            return Items?.Any(a => a.IsListened == false) == true;
        }
    }

    public int? FirstNotListenedEpisodePosition
    {
        get
        {
            var items = Items;

            if (items is null || ListenStateManager is null)
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (!ListenStateManager.IsFullyListen(items[i].Id))
                {
                    return i;
                }
            }

            return null;
        }
    }

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        try
        {
            HasMoreEpisodes = false;

            if (ProgramId < 0)
            {
                DownloadState = DownloadStates.Error;
                return;
            }

            if (refreshSettings.FullRefresh)
            {
                ClearLists();
            }

            DownloadState = DownloadStates.Downloading;

            OnPropertyChanged(nameof(CanAutoCreatePlayList));
            AutoCreatePlayListCommand.NotifyCanExecuteChanged();
            
            var result = await DataFetcher.GetEpisodesAsync(ProgramId, DownloadAllEpisodesOnRefresh, refreshSettings.AllowCache, cancellationToken);

            if (result.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.Error)
            {
                DownloadState = DownloadStates.Error;
                return;
            }

            HasMoreEpisodes = result.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.GotSomePart;

            List<EpisodeModel> temp = new List<EpisodeModel>();
            foreach (var c in result.Episodes!)
            {
                EpisodeModel data = EpisodeModelFactory.Create(c);

                temp.Add(data);
            }

            UpdateList(temp);
            OnPropertyChanged(nameof(FirstNotListenedEpisodePosition));

            if (Items?.Count > 0)
            {
                DownloadState = DownloadStates.Done;
            }
            else
            {
                DownloadState = DownloadStates.NoData;
            }
        }
        finally
        {
            OnPropertyChanged(nameof(CanAutoCreatePlayList));
            AutoCreatePlayListCommand.NotifyCanExecuteChanged();
        }
    }

    protected override int GetId(EpisodeModel item) => item.Id;

    protected override int Compare(EpisodeModel a, EpisodeModel b)
    {
        int multiplier = EpisodesSortOrderManager.IsFavorite(ProgramId) ? 1 : -1;

        if (a.PublishLength.PublishDate is null)
        {
            return 1 * multiplier;
        }

        if (b.PublishLength.PublishDate is null)
        {
            return -1 * multiplier;
        }

        return a.PublishLength.PublishDate.Value.CompareTo(b.PublishLength.PublishDate.Value) * multiplier;
    }

    protected override string GetGroupName(EpisodeModel item) =>
        item.PublishLength.PublishDate is null ? "?" : item.PublishLength.PublishDate.Value.ToString("yyyy-MM");

    protected override int GetGroupPriority(EpisodeModel item)
    {
        int multiplier = EpisodesSortOrderManager.IsFavorite(ProgramId) ? 1 : -1;

        if (item.PublishLength.PublishDate is null)
        {
            return 1 * multiplier;
        }

        return (item.PublishLength.PublishDate.Value.Year * 100 + item.PublishLength.PublishDate.Value.Month) * multiplier;
    }

    public List<SearchItem>? Search(string query)
    {
        var items = Items;

        if (items is null)
        {
            return null;
        }

        return items.
                Where(a => a.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase)).
                Select(i => new SearchItem
                (
                    SearchItemType.Episode,
                    i.Id,
                    i.Title,
                    i.Description,
                    i.EpisodeImage
                )).ToList();
    }


    #endregion

    #region Commands

    [RelayCommand(CanExecute = nameof(HasMoreEpisodes))]
    private void DownloadAllEpisodes()
    {
        DownloadAllEpisodesOnRefresh = true;
        QueueRefresh(new RefreshSettings());
    }

    [RelayCommand(CanExecute = nameof(CanAutoCreatePlayList))]
    public void AutoCreatePlayList()
    {
        if (Items == null)
        {
            return;
        }
        
        List<PlayListItem> playListItems = new List<PlayListItem>();

        foreach (var episode in Items.Where(a => !a.IsListened && a.HasAudio))
        {
            playListItems.Add(episode.CreatePlayListItem());
            if (playListItems.Count >= 10)
            {
                break;
            }
        }

        if (playListItems.Count > 0)
        {
            AudioManager.Play(playListItems.ToArray());
        }
    }

    #endregion
}
