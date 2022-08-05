namespace Pekspro.RadioStorm.UI.Model.Episode;

public record RecentEpisodeModel(EpisodeModel Model, DateTime LatestListenTime, DateOnly DatePeriod, string RecentPeriod);
