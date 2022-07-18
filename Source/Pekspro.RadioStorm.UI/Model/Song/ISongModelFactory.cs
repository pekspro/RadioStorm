namespace Pekspro.RadioStorm.UI.Model.Song;

public interface ISongModelFactory
{
    SongModel Create(SongListItemData songListItemData, EpisodeData? episodeData);
}
