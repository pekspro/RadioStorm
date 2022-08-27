namespace Pekspro.RadioStorm.Audio.Models;

sealed public class PlayListItem
{
    public string? StreamUrl { get; set; }
    internal string? LocalUrl { get; set; }
    public string? Channel { get; set; }
    public string? Program { get; set; }
    public string? Episode { get; set; }
    public string? IconUri { get; set; }
    public int AudioId { get; set; }
    public int ProgramId { get; set; }
    public bool IsLiveAudio { get; set; }
    public int NextPlayPosition { get; set; } = -1;
    public bool SetNextPlayPositionWithNoMargin { get; set; }

    public string? PreferablePlayUrl
    {
        get
        {
            return LocalUrl ?? StreamUrl;
        }
    }

    public bool RequiresInternet
    {
        get
        {
            if (IsLiveAudio)
            {
                return true;
            }

            if (PreferablePlayUrl?.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) == true)
            {
                return true;
            }

            return false;
        }
    }

    public override string ToString()
        => $"Channel: {Channel} Program: {Program} Episode: {Episode} Play-URL: {PreferablePlayUrl}";
}
