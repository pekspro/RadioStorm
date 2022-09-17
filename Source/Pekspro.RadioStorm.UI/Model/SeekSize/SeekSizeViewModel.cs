namespace Pekspro.RadioStorm.UI.Model.SeekSize;

public sealed class SeekSizeViewModel : ObservableObject
{
    #region Private properties

    private SeekSizeProvider SeekSizeProvider = new SeekSizeProvider();

    #endregion

    #region Constructor

    public SeekSizeViewModel()
    {
        SeekSizeProvider.SeekPositionChanged = () =>
        {
            OnPropertyChanged(nameof(RewindSize));
            OnPropertyChanged(nameof(ForwardSize));
        };
    }

    #endregion

    #region Properties

    public TimeSpan RewindSize => SeekSizeProvider.RewindSize;

    public TimeSpan ForwardSize => SeekSizeProvider.ForwardSize;

    #endregion

    #region Methods

    public void Increase()
    {
        SeekSizeProvider.Increase();
    }

    public void Decrease()
    {
        SeekSizeProvider.Decrease();
    }

    #endregion
}
