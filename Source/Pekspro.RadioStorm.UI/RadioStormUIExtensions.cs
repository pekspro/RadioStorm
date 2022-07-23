using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pekspro.RadioStorm.UI;

public static class RadioStormUIExtensions
{
    public static IServiceCollection AddRadioStormUI(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IChannelModelFactory, ChannelModelFactory>();
        services.TryAddSingleton<IProgramModelFactory, ProgramModelFactory>();
        services.TryAddSingleton<IEpisodeModelFactory, EpisodeModelFactory>();
        services.TryAddSingleton<ISongModelFactory, SongModelFactory>();
        services.TryAddSingleton<ISchedulesEpisodeFactory, SchedulesEpisodeFactory>();
        services.TryAddSingleton<IRefreshTimerHelper, RefreshTimerHelper>();
        services.TryAddSingleton<IWeekdaynameHelper, WeekdaynameHelper>();
        services.TryAddSingleton<PlayerViewModel, PlayerViewModel>();
        services.TryAddSingleton<CurrentPlayingViewModel, CurrentPlayingViewModel>();
        services.TryAddTransient<IChannelRefreshHelper, ChannelRefreshHelper>();
        services.TryAddTransient<ChannelDetailsViewModel, ChannelDetailsViewModel>();
        services.TryAddTransient<ChannelsViewModel, ChannelsViewModel>();
        services.TryAddTransient<ProgramDetailsViewModel, ProgramDetailsViewModel>();
        services.TryAddTransient<ProgramSettingsViewModel, ProgramSettingsViewModel>();
        services.TryAddTransient<ProgramsViewModel, ProgramsViewModel>();
        services.TryAddTransient<PlaylistViewModel, PlaylistViewModel>();
        services.TryAddTransient<EpisodeDetailsViewModel, EpisodeDetailsViewModel>();
        services.TryAddTransient<EpisodesViewModel, EpisodesViewModel>();
        services.TryAddTransient<DownloadsViewModel, DownloadsViewModel>();
        services.TryAddTransient<RecentEpisodesViewModel, RecentEpisodesViewModel>();
        services.TryAddTransient<FavoritesViewModel, FavoritesViewModel>();
        services.TryAddTransient<SettingsViewModel, SettingsViewModel>();
        services.TryAddTransient<DebugSettingsViewModel, DebugSettingsViewModel>();
        services.TryAddTransient<AboutViewModel, AboutViewModel>();
        services.TryAddTransient<DownloadSettingsViewModel, DownloadSettingsViewModel>();
        services.TryAddTransient<LoggingViewModel, LoggingViewModel>();
        services.TryAddTransient<SynchronizingViewModel, SynchronizingViewModel>();
        services.TryAddTransient<SongsViewModel, SongsViewModel>();
        services.TryAddTransient<SchedulesEpisodesViewModel, SchedulesEpisodesViewModel>();

        // IUrlLauncher needs to be implemented somewhere else.

        return services;
    }
}
