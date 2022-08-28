namespace Pekspro.RadioStorm.UI.Model.Favorite;

public partial class FavoriteBaseModel : ObservableObject
{
    #region Private properties
    
    private IFavoriteList FavoriteList { get; }
    
    protected ILogger Logger { get; }

    #endregion

    #region Constructor

    public FavoriteBaseModel(IFavoriteList favoriteList, string name, ILogger logger)
    {
        FavoriteList = favoriteList;
        Name = name;
        Logger = logger;
    }

    #endregion

    #region Properties

    public int Id { get; set; }

    public string Name { get; }
    
    public bool IsFavorite
    {
        get
        {
            return FavoriteList?.IsFavorite(Id) == true;
        }
        set
        {
            Logger.LogInformation($"Setting item {0} to favorite: {1}", Id, value);
            FavoriteList.SetFavorite(Id, value);
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    public void ToggleIsFavorite()
    {
        IsFavorite = !IsFavorite;
    }

    #endregion
}
