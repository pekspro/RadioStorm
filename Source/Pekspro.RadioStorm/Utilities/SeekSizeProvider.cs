namespace Pekspro.RadioStorm.Utilities;

public class SeekSizeProvider
{

    public static List<TimeSpan> SeekSizes = new List<TimeSpan>()
    {
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5)
    };

    public TimeSpan RewindSize
    {
        get
        {
            if (SeekPosition <= 0)
            {
                return -SeekSizes[-SeekPosition];
            }

            return -SeekSizes[SeekPosition - 1];
        }
    }

    public TimeSpan ForwardSize
    {
        get
        {
            if( SeekPosition >= 0)
            {
                return SeekSizes[SeekPosition];
            }

            int reversedPos = -SeekPosition;

            return SeekSizes[reversedPos - 1];
        }
    }

    public Action? SeekPositionChanged;

    private int _SeekPosition;
    
    public int SeekPosition
    {
        get => _SeekPosition;
        
        private set
        {
            if (_SeekPosition != value)
            {
                _SeekPosition = value;
                SeekPositionChanged?.Invoke();
            }
        }
    }

    public void Increase()
    {
        Change(1);
    }

    public void Decrease()
    {
        Change(-1);
    }

    private int ChangeId;

    private async void Change(int v)
    {
        ChangeId++;
     
        int currentChangeId = ChangeId;

        int newPos = Math.Clamp(SeekPosition + v, -SeekSizes.Count + 1, SeekSizes.Count - 1);

        if (SeekPosition != newPos)
        {
            SeekPosition = newPos;
        }

        await Task.Delay(2000);

        if (currentChangeId == ChangeId)
        {
            SeekPosition = 0;
        }
    }
}
