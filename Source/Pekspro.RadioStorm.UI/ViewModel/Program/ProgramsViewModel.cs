using Pekspro.RadioStorm.UI.Model.Program;

namespace Pekspro.RadioStorm.UI.ViewModel.Program;

public sealed partial class ProgramsViewModel : ListViewModel<ProgramModel>, ISearch
{
    #region Private properties

    private IDataFetcher DataFetcher { get; }
    private IProgramModelFactory ProgramModelFactory { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Only used in designer.
    /// </summary>
    public ProgramsViewModel()
        : base(null!, null!)
    {
        DataFetcher = null!;
        ProgramModelFactory = null!;
        DownloadState = DownloadStates.Done;

        Items = new ObservableCollection<ProgramModel>();
        Items.Add(ProgramModel.CreateWithSampleData(0));
        Items.Add(ProgramModel.CreateWithSampleData(1));
        Items.Add(ProgramModel.CreateWithSampleData(2));
    }

    public ProgramsViewModel(
        IDataFetcher dataFetcher,
        IProgramModelFactory programModelFactory,
        IMainThreadRunner mainThreadRunner,
        ILogger<ProgramDetailsViewModel> logger)
         : base(logger, mainThreadRunner)
    {
        DataFetcher = dataFetcher;
        ProgramModelFactory = programModelFactory;
    }

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

        var programs = await DataFetcher.GetProgramsAsync(refreshSettings.AllowCache, cancellationToken);

        if (programs is null || !programs.Any())
        {
            DownloadState = DownloadStates.Error;
            return;
        }

        List<ProgramModel> temp = new List<ProgramModel>();
        foreach (var c in programs.OrderBy(a => a.Name))
        {
            ProgramModel data = ProgramModelFactory.Create(c);

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

    protected override int GetId(ProgramModel item) => item.Id;

    protected override int Compare(ProgramModel a, ProgramModel b) => a.Name.CompareTo(b.Name);

    protected override string GetGroupName(ProgramModel item) => 
        string.IsNullOrEmpty(item.CategoryName) ? Strings.Programs_Category_Miscellaneous : item.CategoryName;

    public List<SearchItem>? Search(string query)
    {
        var items = Items;

        if (items is null)
        {
            return null;
        }

        return items.
                Where
                (
                    a => a.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                         a.Description.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                )
                .Select(i => new SearchItem
                (
                    SearchItemType.Program,
                    i.Id,
                    i.Name,
                    i.Description,
                    i.ProgramImage
                )).ToList();
    }
    
    #endregion

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
    //				T.Programs_AddMultipleFavoritesWarning,
    //				SelectionHelper.SelectedItemsCount
    //			);

    //		var messageDialog = new MessageDialog(text, T.Programs_AddMultipleFavoritesWarning_Title);

    //		var acceptCommand = new UICommand(T.General_Yes);
    //		messageDialog.Commands.Add(acceptCommand);
    //		messageDialog.Commands.Add(new UICommand(T.General_No));
    //		messageDialog.DefaultCommandIndex = 1;

    //		var commandChosen = await messageDialog.ShowAsync();

    //		if (commandChosen != acceptCommand)
    //			return;
    //	}

    //	var f = ServiceLocator.Current.GetInstance<FavoriteManager>().Programs;

    //	foreach (var i in SelectionHelper.SelectedItems)
    //	{
    //		var cm = i as ProgramModel;
    //		if (cm is not null)
    //		{
    //			cm.SetIsFavorite(true, FavoriteListChangeSource.Selection);
    //		}
    //	}


    //	SelectionHelper.StopSelectionMode();
    //}

    //#endregion


    //#region GroupedPrograms property

    //private ObservableCollection<ProgramGroup>? _GroupedPrograms = new ObservableCollection<ProgramGroup>();

    //      public ObservableCollection<ProgramGroup>? GroupedPrograms
    //{
    //	get
    //	{
    //		return _GroupedPrograms;
    //	}
    //	set
    //	{
    //		SetProperty(ref _GroupedPrograms, value);
    //	}
    //}

    //      #endregion


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
		private void UpdateFilteredPrograms()
		{
			string filter = Search.SearchText;

			if (string.IsNullOrWhiteSpace(filter))
				FilteredPrograms = Programs;
			else
				FilteredPrograms =
					new ObservableCollection<ProgramModel>
					(
						from c in Programs
						where c.Title.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
								(c.Status?.CurrentProgram != null && c.Status.CurrentProgram.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
								(c.Status?.NextProgram != null && c.Status.NextProgram.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
						select c
					);
			UpdateProgramGroups();
		}
		*/

    //      private void UpdateProgramGroups()
    //{
    //	if (FilteredPrograms is not null)
    //	{
    //		var groupedData =
    //			(from f in FilteredPrograms
    //			 group f by f.ProgramGroupName ?? "?" into g
    //			 //orderby g.Key
    //			 select new ProgramGroup { Header = g.Key, Items = new ObservableCollection<ProgramModel>(g.OrderBy(a => a.Title)) }
    //			 ).ToList();

    //		GroupedPrograms = new ObservableCollection<ProgramGroup>(groupedData);
    //	}
    //}
}
