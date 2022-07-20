using Pekspro.RadioStorm.UI.Model.Favorite;
using Pekspro.SwedRadio;

namespace Pekspro.RadioStorm.UI.ViewModel.Favorite;

public partial class FavoritesViewModel : ListViewModel<FavoriteBaseModel>, IDisposable
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IChannelModelFactory ChannelModelFactory { get; }
    private IProgramModelFactory ProgramModelFactory { get; }
    private IEpisodesSortOrderManager EpisodesSortOrderManager { get; }
    private IListenStateManager ListenStateManager { get; }
    private IEpisodeModelFactory EpisodeModelFactory { get; }
    private IChannelFavoriteList ChannelFavoriteList { get; }
    private IProgramFavoriteList ProgramFavoriteList { get; }
    private IAudioManager AudioManager { get; }
    private IRefreshTimerHelper RefreshTimeHelper { get; }
    private IChannelRefreshHelper ChannelRefreshHelper { get; }
    private IUriLauncher UriLauncher { get; }
    private IMessenger Messenger { get; }
    private IBootstrapState BootstrapState { get; }
    private IDateTimeProvider DateTimeProvider { get; }
    
    private readonly TimeSpan TopEpisodeCacheTime = TimeSpan.FromMinutes(10);

    private Dictionary<int, DateTime> TopEpisodeExpireTime = new Dictionary<int, DateTime>();

    private const int ChannelGroupPriority = 1;

    private const int ProgramGroupPriority = 2;
    
    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public FavoritesViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        ChannelModelFactory = null!;
        AudioManager = null!;
        RefreshTimeHelper = null!;
        ChannelRefreshHelper = null!;
        ProgramModelFactory = null!;
        ChannelFavoriteList = null!;
        ProgramFavoriteList = null!;
        EpisodesSortOrderManager = null!;
        ListenStateManager = null!;
        EpisodeModelFactory = null!;
        UriLauncher = null!;
        Messenger = null!;
        BootstrapState = null!;
        DateTimeProvider = null!;
        DownloadState = DownloadStates.Done;

        var channelGroup = new Group<FavoriteBaseModel>(Strings.Favorites_Group_Channels, ChannelGroupPriority, new List<FavoriteBaseModel>());
        channelGroup.Add(ChannelModel.CreateWithSampleData(0));
        channelGroup.Add(ChannelModel.CreateWithSampleData(1));
        channelGroup.Add(ChannelModel.CreateWithSampleData(2));

        var programGroup = new Group<FavoriteBaseModel>(Strings.Favorites_Group_Programs, ProgramGroupPriority, new List<FavoriteBaseModel>());
        channelGroup.Add(ProgramModel.CreateWithSampleData(0));
        channelGroup.Add(ProgramModel.CreateWithSampleData(1));
        channelGroup.Add(ProgramModel.CreateWithSampleData(2));

        Items = new ObservableCollection<FavoriteBaseModel>();
        GroupedItems = new ObservableCollection<Group<FavoriteBaseModel>>()
        {
            channelGroup,
            programGroup
        };

        foreach (var item in channelGroup)
        {
            Items.Add(item);
        }

        foreach (var item in programGroup)
        {
            Items.Add(item);
        }
    }

    public FavoritesViewModel(
        IDataFetcher dataFetcher,
        IChannelFavoriteList channelFavoriteList,
        IProgramFavoriteList programFavoriteList,
        IChannelModelFactory channelModelFactory,
        IProgramModelFactory programModelFactory,
        IEpisodesSortOrderManager episodesSortOrderManager,
        IListenStateManager listenStateManager,
        IEpisodeModelFactory episodeModelFactory,
        IAudioManager audioManager,
        IRefreshTimerHelper refreshTimeHelper,
        IChannelRefreshHelper channelStatusRefreshHelper,
        IUriLauncher uriLauncher,
        IMessenger messenger,
        IBootstrapState bootstrapState,
        IDateTimeProvider dateTimeProvider,
        IMainThreadRunner mainThreadRunner,
        ILogger<FavoritesViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ChannelFavoriteList = channelFavoriteList;
        ProgramFavoriteList = programFavoriteList;
        ChannelModelFactory = channelModelFactory;
        ProgramModelFactory = programModelFactory;
        EpisodesSortOrderManager = episodesSortOrderManager;
        ListenStateManager = listenStateManager;
        EpisodeModelFactory = episodeModelFactory;
        AudioManager = audioManager;
        RefreshTimeHelper = refreshTimeHelper;
        ChannelRefreshHelper = channelStatusRefreshHelper;
        UriLauncher = uriLauncher;
        Messenger = messenger;
        BootstrapState = bootstrapState;
        DateTimeProvider = dateTimeProvider;
        ChannelRefreshHelper.ViewModel = this;
        ChannelRefreshHelper.ChannelStatusTimer.SetupCallBack(() => QueueRefresh(new RefreshSettings(FullRefresh: false)));
        ChannelRefreshHelper.ChannelProgressTimer.SetupCallBack(() => ChannelRefreshHelper.RefreshChannelProgress(Items));
        messenger?.Register<BootstrapCompleted>(this, (r, m) =>
        {
            Logger.LogDebug("Boot completed - trigger favorite refresh");
            QueueRefresh(new RefreshSettings(FullRefresh: false));
        });

        messenger?.Register<ChannelFavoriteChangedMessage>(this, (r, m) =>
        {
            Logger.LogDebug("ChannelFavoriteChanged - trigger favorite refresh");
            QueueRefresh(new RefreshSettings(FullRefresh: false));
        });

        messenger?.Register<ProgramFavoriteChangedMessage>(this, (r, m) =>
        {
            Logger.LogDebug("ProgramFavoriteChanged - trigger favorite refresh");
            QueueRefresh(new RefreshSettings(FullRefresh: false));
        });

        messenger?.Register<ListenStateChangedMessage>(this, (r, m) =>
        {
            Logger.LogDebug("ListenStateChanged - trigger favorite refresh");
            TopEpisodeExpireTime = new ();
            QueueRefresh(new RefreshSettings(FullRefresh: false));
        });

        messenger?.Register<EpisodeSortOrderChangedMessage>(this, (r, m) =>
        {
            Logger.LogDebug("EpisodeSortOrderChangedMessage - trigger favorite refresh");
            TopEpisodeExpireTime.Remove(m.Id ?? 0);
            QueueRefresh(new RefreshSettings(FullRefresh: false));
        });

    }

    #endregion

    #region Properties

    public bool HasFavorites => DownloadState != DownloadStates.NoData;

    #endregion

    #region Methods

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Refreshing favorites start.");
        Stopwatch sp = Stopwatch.StartNew();

        if (!BootstrapState.BootstrapCompleted)
        {
            DownloadState = DownloadStates.Downloading;
            Logger.LogDebug("Refreshing favorites cancelled, not bootstrapped.");
            return;
        }

        ChannelRefreshHelper.Stop();

        if (refreshSettings.FullRefresh)
        {
            TopEpisodeExpireTime = new Dictionary<int, DateTime>();
        }

        bool updateState = Items is null || !Items.Any() || refreshSettings.FullRefresh || DownloadState == DownloadStates.Error;

        if (updateState)
        {
            DownloadState = DownloadStates.Downloading;
            OnPropertyChanged(nameof(HasFavorites));
            ClearLists();
        }

        if (!await UpdateChannelsAsync(refreshSettings, cancellationToken))
        {
            return;
        }

        if (!await UpdateProgramsAsync(refreshSettings, cancellationToken))
        {
            return;
        }

        try
        {
            ChannelRefreshHelper.RefreshChannelProgress(Items);

            await ChannelRefreshHelper.RefreshChannelStatusAsync
            (
                DataFetcher,
                Items,
                refreshSettings,
                false,
                cancellationToken
            );

            await UpdateProgramTopEpisodeAsync(cancellationToken);
        }
        catch (TaskCanceledException )
        {
            // If task is cancelled here it is probably due settings 
            // has been synhronized and a reload is needed. If we
            // don't suppress this State will be set to Cancelled
            // and this causes flickering in the UI.

            return;
        }

        ChannelRefreshHelper.SetupStatusRefreshTimer(Items);

        if (Items?.Count > 0)
        {
            DownloadState = DownloadStates.Done;

            Messenger.Send(new UiLoaded());
        }
        else
        {
            DownloadState = DownloadStates.NoData;
        }

        OnPropertyChanged(nameof(HasFavorites));

        Logger.LogDebug($"Refreshing favorites completed in {sp.ElapsedMilliseconds} ms.");
    }

    private async Task<bool> UpdateChannelsAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Logger.LogDebug("Updating channels start.");

        var favoriteChannelIds = ChannelFavoriteList.Items.Where(a => a.Value.IsActive).Select(s => s.Value.Id).ToList();

        var channelFavoriteGroup = GroupedItems?.FirstOrDefault(a => a.Priority == ChannelGroupPriority);

        DeleteObselete(c => c is ChannelModel && !favoriteChannelIds.Contains(c.Id));

        if (favoriteChannelIds.Any())
        {
            IList<ChannelData>? fetchedChannels = null;

            foreach (var favoriteId in favoriteChannelIds)
            {
                // Do not do anything if already in list.
                if (channelFavoriteGroup?.Any(a => a.Id == -favoriteId) == true)
                {
                    continue;
                }

                if (fetchedChannels is null)
                {
                    fetchedChannels = await DataFetcher.GetChannelsAsync(refreshSettings.AllowCache, cancellationToken);

                    if (fetchedChannels is null || !fetchedChannels.Any())
                    {
                        DownloadState = DownloadStates.Error;
                        OnPropertyChanged(nameof(HasFavorites));
                        return false;
                    }
                }

                var channelData = fetchedChannels.FirstOrDefault(a => a.ChannelId == favoriteId);

                if (channelData is null)
                {
                    // Invalid favorite id
                    continue;
                }

                ChannelModel model = ChannelModelFactory.Create(channelData);
                Insert(model);

                DownloadState = DownloadStates.Done;
                OnPropertyChanged(nameof(HasFavorites));
            }
        }

        Logger.LogDebug($"Updating channels completed in {stopwatch.ElapsedMilliseconds} ms.");

        return true;
    }

    private async Task<bool> UpdateProgramsAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Logger.LogDebug("Updating programs start.");

        var favoriteProgramIds = ProgramFavoriteList.Items.Where(a => a.Value.IsActive).Select(s => s.Value.Id).ToList();

        var programFavoriteGroup = GroupedItems?.FirstOrDefault(a => a.Priority == ProgramGroupPriority);

        DeleteObselete(c => c is ProgramModel && !favoriteProgramIds.Contains(c.Id));

        if (favoriteProgramIds.Any())
        {
            IList<ProgramData>? fetchedPrograms = null;

            foreach (var favoriteId in favoriteProgramIds)
            {
                // Do not do anything if already in list.
                if (programFavoriteGroup?.Any(a => a.Id == favoriteId) == true)
                {
                    continue;
                }

                if (fetchedPrograms is null)
                {
                    fetchedPrograms = await DataFetcher.GetProgramsAsync(refreshSettings.AllowCache, cancellationToken);

                    if (fetchedPrograms is null || !fetchedPrograms.Any())
                    {
                        DownloadState = DownloadStates.Error;
                        return false;
                    }
                }

                var programData = fetchedPrograms.FirstOrDefault(a => a.ProgramId == favoriteId);

                if (programData is null)
                {
                    // Invalid favorite id
                    continue;
                }

                ProgramModel model = ProgramModelFactory.Create(programData);
                Insert(model);
                
                DownloadState = DownloadStates.Done;
                OnPropertyChanged(nameof(HasFavorites));
            }
        }

        Logger.LogDebug($"Updating programs completed in {stopwatch.ElapsedMilliseconds} ms.");

        return true;
    }

    private async Task UpdateProgramTopEpisodeAsync(CancellationToken cancellation)
    {
        if (Items is null)
        {
            return;
        }

        List<ProgramModel> items = Items.Where(a => a is ProgramModel).Select(a => (ProgramModel) a)!.ToList();

        Stopwatch stopwatch = Stopwatch.StartNew();
        Logger.LogDebug($"Updating program top episodes start. {items.Count} programs.");

        // First set a quick result.
        foreach (ProgramModel p in items.Where(a => a.EpisodeStatus == null))
        {
            bool sortDescending = !EpisodesSortOrderManager.IsFavorite(p.Id);

            var listenedIds = ListenStateManager.Items.Where(a => a.Value.IsFullyListen).Select(a => a.Value.Id).ToArray();

            // First get top episode that is not listened.
            var firstCachedEpisode = await DataFetcher.GetFirstMatchingEpisodeFromCacheAsync(new int?[] { p.Id }, listenedIds, !sortDescending, cancellation);
            bool allListened = false;

            // Otherwise, get top episode that is listened.
            if (firstCachedEpisode is null)
            {
                firstCachedEpisode = await DataFetcher.GetFirstMatchingEpisodeFromCacheAsync(new int?[] { p.Id }, new int[0], !sortDescending, cancellation);
                allListened = true;
            }

            if (firstCachedEpisode is not null)
            {
                ProgramEpisodeStatusModel episodeStatus = new ProgramEpisodeStatusModel()
                {
                    NotListenedEpisodeCount = 0,
                    ForceNotAllEpisodesIsListened = !allListened,
                    TopEpisode = EpisodeModelFactory.Create(firstCachedEpisode)
                };

                p.EpisodeStatus = episodeStatus;
            }
        }

        List<int> programsThatNeedsFullSynchronizing = new List<int>();
        DateTime now = DateTimeProvider.UtcNow;

        // Ignore items with top epsidodes that is valid cache.
        items = items.Where(a => a.EpisodeStatus?.TopEpisode is null ||
                                 !TopEpisodeExpireTime.TryGetValue(a.Id, out DateTime topEpisodeCacheTime) ||
                                 topEpisodeCacheTime < now).ToList();

        //Step 0: Get epsisodes from in descending order
        //Step 1: Get epsisodes from in ascending order
        //Step 2: Get all episodes

        for (int step = 0; step < 3; step++)
        {
            foreach (var p in items)
            {
                bool sortDescending = !EpisodesSortOrderManager.IsFavorite(p.Id);

                if (!sortDescending && step == 0)
                {
                    continue;
                }
                if (sortDescending && step == 1)
                {
                    continue;
                }
                if (step == 2 && !programsThatNeedsFullSynchronizing.Any(a => a == p.Id))
                {
                    continue;
                }

                var getEpisodesResult = await DataFetcher.GetEpisodesAsync(p.Id, programsThatNeedsFullSynchronizing.Any(a => a == p.Id), cancellationToken: cancellation);

                if (step != 2 && getEpisodesResult.Result != RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.Full)
                {
                    if (getEpisodesResult.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.GotSomePart)
                    {
                        programsThatNeedsFullSynchronizing.Add(p.Id);
                    }
                    else if (getEpisodesResult.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.Error)
                    {
                        programsThatNeedsFullSynchronizing.Add(p.Id);
                    }
                    else if (getEpisodesResult.Result == RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.IncrementalUpdate &&
                            CacheTime.IsTimeOut(getEpisodesResult.LatestFullSyncronizingTime, CacheTime.EpisodeListFullSynchronzingIntervall)
                            )
                    {
                        programsThatNeedsFullSynchronizing.Add(p.Id);
                    }
                }

                if (getEpisodesResult.Result != RadioStorm.DataFetcher.DataFetcher.GetListResultEnum.Error && getEpisodesResult.Episodes!.Any())
                {
                    List<EpisodeData> episodes;

                    if (sortDescending)
                    {
                        episodes = getEpisodesResult.Episodes!.OrderByDescending(a => a.PublishDate).ToList();
                    }
                    else
                    {
                        episodes = getEpisodesResult.Episodes!.OrderBy(a => a.PublishDate).ToList();
                    }

                    EpisodeData? firstNotListenedEpisode = null;
                    int notListenedCount = 0;

                    foreach (var e in episodes)
                    {
                        if (!ListenStateManager.IsFullyListen(e.EpisodeId))
                        {
                            if (firstNotListenedEpisode is null)
                            {
                                firstNotListenedEpisode = e;
                            }

                            notListenedCount++;
                        }
                    }

                    EpisodeData? topEpisode = firstNotListenedEpisode;
                    if (topEpisode is null)
                    {
                        topEpisode = episodes[0];
                    }

                    ProgramEpisodeStatusModel episodeStatus = new ProgramEpisodeStatusModel()
                    {
                        NotListenedEpisodeCount = notListenedCount,
                        TopEpisode = EpisodeModelFactory.Create(topEpisode)
                    };

                    p.EpisodeStatus = episodeStatus;
                    TopEpisodeExpireTime[p.Id] = DateTimeProvider.UtcNow.Add(TopEpisodeCacheTime);
                }
            }
        }

        Logger.LogDebug($"Updating {items.Count} program top episodes completed in {stopwatch.ElapsedMilliseconds} ms.");
    }

    protected override int GetId(FavoriteBaseModel item)
    {
        if (item is ChannelModel)
        {
            return -item.Id;
        }

        return item.Id;
    }

    protected override int Compare(FavoriteBaseModel a, FavoriteBaseModel b)
    {
        int aPriority = GetGroupPriority(a);
        int bPriority = GetGroupPriority(b);

        if (aPriority != bPriority)
        {
            return aPriority.CompareTo(bPriority);
        }

        return a.Name.CompareTo(b.Name);
    }

    protected override string GetGroupName(FavoriteBaseModel item)
    {
        if (item is ProgramModel)
        {
            return Strings.Favorites_Group_Programs;
        }
        else if (item is ChannelModel)
        {
            return Strings.Favorites_Group_Channels;
        }
        else
        {
            return "?";
        }
    }

    protected override int GetGroupPriority(FavoriteBaseModel item)
    {
        if (item is ChannelModel)
        {
            return ChannelGroupPriority;
        }

        return ProgramGroupPriority;
    }

    public override void OnNavigatedTo(bool refresh = true)
    {
        base.OnNavigatedTo(refresh);

        ChannelRefreshHelper.RefreshChannelProgress(Items);
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();

        ChannelRefreshHelper.Stop();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void AddRecommendedFavorites()
    {
        DownloadState = DownloadStates.Downloading;
        OnPropertyChanged(nameof(HasFavorites));

        int[] recommendedChannels = new int[]
        {
            132, //P1
				163, //P2
				164, //P3
			};

        int[] recommendPrograms = new int[]
        {
            4131, //Institutet
				909, //P1 Dokumentär
				2519, //P3 Dokumentär
				1307, //På minuten
				2946, //Radiokorrespondenterna
				516, //Spanarna
				411, //Språket
				4572, //Svenska berättelser
				407, //Vetenskapsradion historia
				2071, //Sommar & Vinter i P1
				793, //Filosofiska rummet
				4772, //Söndagsintervjun
				5067, //P3 Historia
				3345	//Klotet
			};

        foreach (int id in recommendedChannels)
        {
            ChannelFavoriteList.SetFavorite(id, true);
        }

        foreach (int id in recommendPrograms)
        {
            ProgramFavoriteList.SetFavorite(id, true);
        }
    }

    [RelayCommand]
    private async void Email()
    {
        await UriLauncher.LaunchAsync(new Uri($"mailto:{Strings.General_Pekspro_EmailAddress}"));
    }

    #endregion


    /*
		protected override bool OnBackRequested()
		{
			if (SelectionHelper.IsInSelectionMode)
			{
				SelectionHelper.StopSelectionMode();
				return true;
			}

			return base.OnBackRequested();
		}
		*/

    //#region SelectionHelper property

    //private ListSelectionHelper _SelectionHelper = new ListSelectionHelper();

    //public ListSelectionHelper SelectionHelper
    //{
    //	get
    //	{
    //		return _SelectionHelper;
    //	}
    //	set
    //	{
    //		Set(ref _SelectionHelper, value);
    //	}
    //}

    //#endregion



    //#region CommandAddMultipleFavorites property

    //private RelayCommand _CommandAddMultipleFavorites;

    //public RelayCommand CommandAddMultipleFavorites
    //{
    //	get
    //	{
    //		return _CommandAddMultipleFavorites = _CommandAddMultipleFavorites ??
    //					new RelayCommand(ExecuteCommandAddMultipleFavorites);
    //	}
    //}

    //private async void ExecuteCommandAddMultipleFavorites()
    //{
    //	if (SelectionHelper.SelectedItemsCount > 1)
    //	{
    //		string text =
    //			string.Format
    //			(
    //				T.Channels_AddMultipleFavoritesWarning,
    //				SelectionHelper.SelectedItemsCount
    //			);

    //		var messageDialog = new MessageDialog(text, T.Channels_AddMultipleFavoritesWarning_Title);

    //		var acceptCommand = new UICommand(T.General_Yes);
    //		messageDialog.Commands.Add(acceptCommand);
    //		messageDialog.Commands.Add(new UICommand(T.General_No));
    //		messageDialog.DefaultCommandIndex = 1;

    //		var commandChosen = await messageDialog.ShowAsync();

    //		if (commandChosen != acceptCommand)
    //			return;
    //	}

    //	var f = ServiceLocator.Current.GetInstance<FavoriteManager>().Channels;

    //	foreach (var i in SelectionHelper.SelectedItems)
    //	{
    //		var cm = i as ChannelModel;
    //		if (cm is not null)
    //		{
    //			cm.SetIsFavorite(true, FavoriteListChangeSource.Selection);
    //		}
    //	}


    //	SelectionHelper.StopSelectionMode();
    //}

    //#endregion

    /*

		#region Search property

		private SearchBoxHelper _Search;

		public SearchBoxHelper Search
		{
			get
			{
				return _Search;
			}
			private set
			{
				Set(ref _Search, value);
			}
		}

		#endregion

		*/

    /*
		private void UpdateFilteredChannels()
		{
			string filter = Search.SearchText;

			if (string.IsNullOrWhiteSpace(filter))
				FilteredChannels = Channels;
			else
				FilteredChannels =
					new ObservableCollection<ChannelModel>
					(
						from c in Channels
						where c.Title.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
								(c.Status?.CurrentProgram != null && c.Status.CurrentProgram.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
								(c.Status?.NextProgram != null && c.Status.NextProgram.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
						select c
					);
			UpdateProgramGroups();
		}
		*/



    //public void UpdateButtons()
    //{
    //	if (GroupedFavorites is not null)
    //		foreach (var v in GroupedFavorites)
    //			v.RaiseCanPlayChanged();
    //}

    #region Dispose

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ChannelRefreshHelper.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
