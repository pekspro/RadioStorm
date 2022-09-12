namespace Pekspro.RadioStorm.UI.ViewModel.Channel;

public class ChannelsViewModel : ListViewModel<ChannelModel>, ISearch, IDisposable
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IChannelModelFactory ChannelModelFactory { get; }
    private IChannelRefreshHelper ChannelRefreshHelper { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public ChannelsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        ChannelModelFactory = null!;
        ChannelRefreshHelper = null!;
        DownloadState = DownloadStates.Done;

        Items = new ObservableCollection<ChannelModel>();
        Items.Add(ChannelModel.CreateWithSampleData(0));
        Items.Add(ChannelModel.CreateWithSampleData(1));
        Items.Add(ChannelModel.CreateWithSampleData(2));
    }

    public ChannelsViewModel(
        IDataFetcher dataFetcher,
        IChannelModelFactory channelModelFactory,
        IChannelRefreshHelper channelStatusRefreshHelper,
        IMainThreadRunner mainThreadRunner,
        ILogger<ChannelDetailsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ChannelModelFactory = channelModelFactory;
        ChannelRefreshHelper = channelStatusRefreshHelper;

        ChannelRefreshHelper.ViewModel = this;
        ChannelRefreshHelper.ChannelStatusTimer.SetupCallBack(() => QueueRefresh(new RefreshSettings(FullRefresh: false)));
        ChannelRefreshHelper.ChannelProgressTimer.SetupCallBack(() => ChannelRefreshHelper.RefreshChannelProgress(Items));
    }

    #endregion

    #region Methods

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

    internal override async Task RefreshAsync(RefreshSettings refreshSettings, CancellationToken cancellationToken)
    {
        ChannelRefreshHelper.Stop();

        if (refreshSettings.FullRefresh)
        {
            ClearLists();
        }

        if (Items is null || !Items.Any())
        {
            DownloadState = DownloadStates.Downloading;

            var channels = await DataFetcher.GetChannelsAsync(refreshSettings.AllowCache, cancellationToken);

            if (channels is null || !channels.Any())
            {
                DownloadState = DownloadStates.Error;
                return;
            }

            List<ChannelModel> temp = new List<ChannelModel>();
            foreach (var c in channels)
            {
                ChannelModel data = ChannelModelFactory.Create(c);

                temp.Add(data);
            }

            UpdateList(temp);

            if (Items?.Count > 0)
            {
                DownloadState = DownloadStates.Done;
            }
            else
            {
                DownloadState = DownloadStates.NoData;
            }
        }

        ChannelRefreshHelper.RefreshChannelProgress(Items);

        await ChannelRefreshHelper.RefreshChannelStatusAsync
            (
                DataFetcher,
                Items,
                refreshSettings,
                true,
                cancellationToken
            );
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
                    SearchItemType.Channel,
                    i.Id,
                    i.Title,
                    null,
                    i.ChannelImage,
                    i.ChannelColor
                )).ToList();
    }

    protected override int GetId(ChannelModel item) => item.Id;

    protected override int Compare(ChannelModel a, ChannelModel b)
    {
        if (a.ChannelGroupPriority != b.ChannelGroupPriority)
        {
            return a.ChannelGroupPriority.CompareTo(b.ChannelGroupPriority);
        }

        return a.Title.CompareTo(b.Title);
    }

    protected override int GetGroupPriority(ChannelModel item) => item.ChannelGroupPriority;

    protected override string GetGroupName(ChannelModel item) => item.ChannelGroupName;

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


    #region Dispose

    private bool disposedValue;

    private void Dispose(bool disposing)
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
