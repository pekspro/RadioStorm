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
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.TryAddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.TryAddSingleton<ILocalSettings, LocalSettings>();
        services.TryAddSingleton<ILogFileNameCreator, LogFileNameCreator>();
        services.TryAddSingleton<IDownloadSettings, DownloadSettings>();
        services.TryAddSingleton<IBootstrapState, BootstrapState>();
        services.TryAddSingleton<IDownloadManager, DownloadManager>();
        services.TryAddSingleton<IDownloadFetcher, DownloadFetcher>();
        services.TryAddTransient<ICachePrefetcher, CachePrefetcher>();
        services.TryAddTransient<IAutoDownloadManager, AutoDownloadManager>();
        services.TryAddTransient<IAutoDownloadDeleteManager, AutoDownloadDeleteManager>();
        services.TryAddTransient<ILogFileHelper, LogFileHelper>();

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
        // * IVersionProvider

        return services;
    }

    public static ILoggingBuilder AddInMemory(this ILoggingBuilder builder)
    {
        // Build service provider
        var serviceProvider = builder.Services.BuildServiceProvider();

        var inMemoryProvider = new InMemLoggerProvider(serviceProvider.GetRequiredService<IDateTimeProvider>());
        builder.Services.TryAddSingleton(inMemoryProvider);
        
        return builder.AddProvider(inMemoryProvider);
    }

    public static ILoggingBuilder AddFileIfEnabled(this ILoggingBuilder builder)
    {
        // Build service provider
        var serviceProvider = builder.Services.BuildServiceProvider();

        var localSettings = serviceProvider.GetRequiredService<ILocalSettings>();

        if (!localSettings.WriteLogsToFile)
        {
            return builder;
        }

        // Create file name
        var logFileName = serviceProvider.GetRequiredService<ILogFileNameCreator>().CreateLogFilName("main");

        // Open file
        var fileStream = new FileStream(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read);

        // Create FileLoggerProvder
        var fileLogger = new FileLoggerProvider
            (
                serviceProvider.GetRequiredService<IDateTimeProvider>(),
                fileStream
            );

        builder.Services.TryAddSingleton(fileLogger);
        
        return builder.AddProvider(fileLogger);
    } 
}
