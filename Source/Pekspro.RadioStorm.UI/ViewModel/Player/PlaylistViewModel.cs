namespace Pekspro.RadioStorm.UI.ViewModel.Player;

public partial class PlaylistViewModel : DownloadViewModel
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IProgramModelFactory ProgramModelFactory { get; }
    private IAudioManager AudioManager { get; }
    private IDateTimeProvider DateTimeProvider { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public PlaylistViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        EpisodeModelFactory = null!;
        ProgramModelFactory = null!;
        AudioManager = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        Items.Add(EpisodeModel.CreateWithSampleData(0));
        Items.Add(EpisodeModel.CreateWithSampleData(1));
        Items.Add(EpisodeModel.CreateWithSampleData(2));
    }

    public PlaylistViewModel(
        IDataFetcher dataFetcher,
        IEpisodeModelFactory episodeModelFactory,
        IProgramModelFactory programModelFactory,
        IAudioManager audioManager,
        IMessenger messenger,
        IDateTimeProvider dateTimeProvider,
        IMainThreadRunner mainThreadRunner,
        ILogger<PlaylistViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        EpisodeModelFactory = episodeModelFactory;
        ProgramModelFactory = programModelFactory;
        AudioManager = audioManager;
        DateTimeProvider = dateTimeProvider;
        messenger.Register<PlaylistChanged>(this, (r, m) =>
        {
            if (!m.ItemsMoved)
            {
                QueueRefresh(new RefreshSettings());
            }
        }
        );

        Items.CollectionChanged += Items_CollectionChanged;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private ObservableCollection<EpisodeModel> _Items = new ObservableCollection<EpisodeModel>();

    [ObservableProperty]
    private bool _HasPlayList;

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        DownloadState = DownloadStates.Downloading;

        Items.Clear();

        var items = AudioManager.CurrentPlayList;

        HasPlayList = items is not null && !items.IsLiveAudio && items.Items.Any();

        if (!HasPlayList || items is null)
        {
            DownloadState = DownloadStates.NoData;
            return;
        }

        List<EpisodeModel> pendingItemsToAdd = new();
        DateTime latestAddTime = DateTimeProvider.UtcNow;
        DownloadState = DownloadStates.Downloading;

        void AddFetchedItems()
        {
            foreach (var item in pendingItemsToAdd)
            {
                Items.Add(item);

                DownloadState = DownloadStates.Done;
            }

            pendingItemsToAdd.Clear();
        }

        for (int i = 0; i < items.Items.Count; i++)
        {
            var item = items.Items[i];

            var id = item.AudioId;

            var dbEpisodeData = await DataFetcher.GetEpisodeAsync(id, refreshSettings.AllowCache, cancellationToken);

            if (dbEpisodeData is not null)
            {
                EpisodeModel episodeModel = EpisodeModelFactory.Create(dbEpisodeData);
                episodeModel.SetAsActivePlaylistItem = true;

                pendingItemsToAdd.Add(episodeModel);
            }

            DateTime now = DateTimeProvider.UtcNow;
            if (i >= items.Items.Count - 1 || (now - latestAddTime).TotalMilliseconds >= 1000)
            {
                latestAddTime = now;
                AddFetchedItems();
            }
        }

        AddFetchedItems();

        if (Items.Count > 0)
        {
            DownloadState = DownloadStates.Done;
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }

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

    public void RemoveFromPlaylist(EpisodeModel episodeModel)
    {
        AudioManager.RemoveItemFromPlaylist(episodeModel.Id);
    }

    public void RemoveFromPlaylist(EpisodeModel[] episodeModels)
    {
        AudioManager.RemoveItemsFromPlaylist(episodeModels.Select(a => a.Id));
    }

    private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
        {
            List<EpisodeModel> oldModels = new();
            List<EpisodeModel> newModels = new();

            foreach (var i in e.OldItems!)
            {
                if (i is EpisodeModel el)
                {
                    oldModels.Add(el);
                }
            }

            foreach (var j in e.NewItems!)
            {
                if (j is EpisodeModel el)
                {
                    newModels.Add(el);
                }
            }

            newModels.Clear();

            AudioManager.MovePlaylistItem(e.OldStartingIndex, e.NewStartingIndex);
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //RemoveIndex = e.OldStartingIndex;
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) // && RemoveIndex >= 0 && RemoveIndex < PlaylistItems.Count)
        {
            //AudioManager.MovePlaylistItem(RemoveIndex, e.NewStartingIndex);

            //RemoveIndex = -1;
        }
    }

    #endregion
}
