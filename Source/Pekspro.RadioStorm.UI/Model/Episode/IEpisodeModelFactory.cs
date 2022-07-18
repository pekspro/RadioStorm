namespace Pekspro.RadioStorm.UI.Model.Episode
{
    public interface IEpisodeModelFactory
    {
        EpisodeModel Create(EpisodeData episodeData);
    }
}