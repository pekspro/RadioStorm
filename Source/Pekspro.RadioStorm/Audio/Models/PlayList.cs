namespace Pekspro.RadioStorm.Audio.Models;

public sealed class PlayList
{
    public Guid PlayListId { get; set; }
    public List<PlayListItem> Items { get; set; } = new List<PlayListItem>();
    public int CurrentPosition { get; set; }

    public PlayListItem? CurrentItem
    {
        get
        {
            if (CurrentPosition < 0)
            {
                return null;
            }

            if (CurrentPosition >= Items.Count)
            {
                return null;
            }

            return Items[CurrentPosition];
        }
    }

    public bool IsLiveAudio
    {
        get
        {
            if (Items is null || CurrentPosition < 0 || CurrentPosition >= Items.Count)
            {
                return false;
            }

            return Items[CurrentPosition].IsLiveAudio;
        }
    }

    public bool RequiresInternet => Items?.Any(a => a.RequiresInternet) == true;

    public bool CanGoToNext => Items.Count > 1 && CurrentPosition + 1 < Items.Count;

    public bool CanGoToPrevious => Items.Count > 1 && CurrentPosition > 0;
}
