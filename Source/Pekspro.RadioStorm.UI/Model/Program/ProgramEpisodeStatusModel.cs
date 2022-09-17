namespace Pekspro.RadioStorm.UI.Model.Program;

public sealed partial class ProgramEpisodeStatusModel : ObservableObject
{
    #region Constructor
    
    public ProgramEpisodeStatusModel()
    {
    }

    #endregion
    
    #region Properites

    [ObservableProperty]
    private EpisodeModel? _TopEpisode;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNotListenedEpisodes))]
    [NotifyPropertyChangedFor(nameof(AllEpisodesIsListened))]
    private int _NotListenedEpisodeCount;

    public bool HasNotListenedEpisodes
    {
        get
        {
            return NotListenedEpisodeCount > 0;
        }
    }

    public bool AllEpisodesIsListened
    {
        get
        {
            if (ForceNotAllEpisodesIsListened)
            {
                return false;
            }

            return NotListenedEpisodeCount <= 0;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AllEpisodesIsListened))]
    private bool _ForceNotAllEpisodesIsListened;

    #endregion
}
