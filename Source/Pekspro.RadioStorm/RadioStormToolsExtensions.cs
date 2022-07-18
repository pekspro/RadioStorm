using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pekspro.RadioStorm;

public static class RadioStormToolsExtensions
{
    public static IServiceCollection AddRadioStorm(this IServiceCollection services, IConfiguration Configuration)
    {
        IConfigurationSection configurationSection = Configuration.GetSection("StorageLocation");
        services.Configure<StorageLocations>(configurationSection);

        services.AddLogging();

        services.TryAddTransient<Bootstrap.Bootstrap, Bootstrap.Bootstrap>();
        services.TryAddSingleton<IDtoConverter, DtoConverter>();
        services.TryAddSingleton<ICacheDatabaseContextFactory, CacheDatabaseContextFactory>();
        services.TryAddTransient<CacheDatabaseManager, CacheDatabaseManager>();
        services.TryAddTransient<CacheDatabaseHelper, CacheDatabaseHelper>();
        services.TryAddSingleton<IGeneralDatabaseContextFactory, GeneralDatabaseContextFactory>();
        services.TryAddTransient<GeneralDatabaseHelper, GeneralDatabaseHelper>();
        services.TryAddTransient<IShutDownManager, ShutDownManager>();
        services.TryAddSingleton<IVersionProvider, VersionProvider>();
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.TryAddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.TryAddSingleton<ILocalSettings, LocalSettings>();
        services.TryAddSingleton<IDownloadSettings, DownloadSettings>();
        services.TryAddSingleton<IBootstrapState, BootstrapState>();
        services.TryAddSingleton<IDownloadManager, DownloadManager>();
        services.TryAddSingleton<IDownloadFetcher, DownloadFetcher>();
        services.TryAddTransient<ICachePrefetcher, CachePrefetcher>();
        services.TryAddTransient<IAutoDownloadManager, AutoDownloadManager>();
        services.TryAddTransient<IAutoDownloadDeleteManager, AutoDownloadDeleteManager>();

        services.TryAddSingleton<IListenStateManager, ListenStateManager>();
        services.TryAddSingleton<IEpisodesSortOrderManager, EpisodesSortOrderManager>();
        services.TryAddSingleton<IRecentPlayedManager, RecentPlayedManager>();
        services.TryAddSingleton<IProgramFavoriteList, ProgramFavoriteList>();
        services.TryAddSingleton<IChannelFavoriteList, ChannelFavoriteList>();
        services.TryAddSingleton<ISharedSettingsManager, SharedSettingsManager>();
        services.TryAddTransient<IDataFetcher, DataFetcher.DataFetcher>();
        services.TryAddTransient<IMainThreadTimerFactory, MainThreadTimerFactory>();

        // These services needs to be implemented somewhere else:
        // * ISettingsService
        // * IAudioManager
        // * IMainThreadTimer

        return services;
    }

    public static ILoggingBuilder AddInMemory(this ILoggingBuilder builder)
    {
        var logger = new InMemoryLogger(new DateTimeProvider());
        builder.Services.TryAddSingleton(logger);
        return builder.AddProvider(new InMemLoggerProvider(logger));
    }
}
