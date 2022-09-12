namespace Pekspro.RadioStorm.UI.Model.Episode;

public sealed record RecentEpisodeModel(EpisodeModel Model, DateTime LatestListenTime, DateOnly DatePeriod, string RecentPeriod);
