﻿using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pekspro.RadioStorm.MAUI.Pages.Favorite;
using Pekspro.RadioStorm.MAUI.Pages.Recent;
using Pekspro.RadioStorm.Options;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider;
using Pekspro.RadioStorm.UI;

namespace Pekspro.RadioStorm.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                //fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                //fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            })
            ;

        // TODO: Remove when fixed: https://github.com/dotnet/efcore/issues/32346
        System.AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue31751", true);

#if WINDOWS
        var currentApplicationData = global::Windows.Storage.ApplicationData.Current;

        builder.Services.Configure<StorageLocations>(a => 
            a.Configure
            (
                Path.GetDirectoryName(currentApplicationData.LocalFolder.Path)!,
                currentApplicationData.LocalFolder.Path,
                currentApplicationData.LocalCacheFolder.Path,
                currentApplicationData.TemporaryFolder.Path
            ));
#elif ANDROID
        string baseStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string cachePath = Android.App.Application.Context.CacheDir!.AbsolutePath;
        string temporaryPath = Path.Combine(cachePath, "temp");
        builder.Services.Configure<StorageLocations>(a => a.ConfigureFromBasePath(baseStoragePath, cachePath, temporaryPath));
#else
        string baseStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        builder.Services.Configure<StorageLocations>(a => a.ConfigureFromBasePath(baseStoragePath));
#endif
        builder.Services.TryAddSingleton<IMainThreadRunner, MainThreadRunner>();
        builder.Services.TryAddSingleton<IUriLauncher, UriLauncher>();
        builder.Services.TryAddSingleton<ISettingsService, SettingsService>();
        builder.Services.TryAddSingleton<IConnectivityProvider, ConnectivityProvider>();
        builder.Services.TryAddSingleton<IVersionProvider, Services.VersionProvider>();
#if WINDOWS
        builder.Services.TryAddTransient<IVersionMigrator, Pekspro.RadioStorm.MAUI.Platforms.Windows.Services.VersionMigrator>();
        builder.Services.TryAddSingleton<IAudioManager, WindowsAudioManager>();
        builder.Services.TryAddSingleton<IReviewLauncher, Pekspro.RadioStorm.MAUI.Platforms.Windows.Services.ReviewLauncherWindows>();
        builder.Services.TryAddSingleton<INotificationPermissionManager, NotificationPermissionManagerDefault>();
        builder.Services.AddRadioStormFileProviders(builder.Configuration, true);
#elif ANDROID
        builder.Services.TryAddSingleton<IAudioManager, AndroidAudioManager>();
        builder.Services.TryAddSingleton<IReviewLauncher, Pekspro.RadioStorm.MAUI.Platforms.Android.Services.ReviewLauncherAndroid>();
        builder.Services.TryAddSingleton<INotificationPermissionManager, Pekspro.RadioStorm.MAUI.Platforms.Android.Services.NotificationPermissionManagerAndroid>();
        builder.Services.AddRadioStormFileProviders(builder.Configuration, false);
#endif
        builder.Services.AddRadioStorm(builder.Configuration);
        builder.Services.AddRadioStormUI(builder.Configuration);

        builder.Services.TryAddTransient<ChannelsPage>();
        builder.Services.TryAddTransient<ChannelDetailsPage>();
        builder.Services.TryAddTransient<ScheduledEpisodesPage>();
        builder.Services.TryAddTransient<ChannelSongListPage>();
        builder.Services.TryAddTransient<DownloadsPage>();
        builder.Services.TryAddTransient<EpisodeDetailsPage>();
        builder.Services.TryAddTransient<EpisodeSongListPage>();
        builder.Services.TryAddTransient<FavoritesPage>();
        builder.Services.TryAddTransient<PlaylistPage>();
        builder.Services.TryAddTransient<CurrentPlayingPage>();
        builder.Services.TryAddTransient<ProgramsPage>();
        builder.Services.TryAddTransient<ProgramDetailsPage>();
        builder.Services.TryAddTransient<ProgramSettingsPage>();
        builder.Services.TryAddTransient<RecentEpisodesPage>();
        builder.Services.TryAddTransient<AboutPage>();
        builder.Services.TryAddTransient<SettingsPage>();
        builder.Services.TryAddTransient<LogFileDetailsPage>();

#if DEBUG
        builder.Services.AddLogging(a =>
        {
#if WINDOWS
            a.AddDebug()
             .AddFilter("Microsoft", LogLevel.Warning)
             .AddFilter("Pekspro.RadioStorm", LogLevel.Debug);
#endif

#if ANDROID
            a.AddProvider(new AndroidLoggerProvider())
             .AddFilter("Microsoft", LogLevel.Warning)
             .AddFilter("Pekspro.RadioStorm", LogLevel.Debug);
#endif

            a.AddFileIfEnabled();
        });
#else
            builder.Services.AddLogging(a =>
        {
            a.AddFileIfEnabled()
             .AddFilter("Microsoft", LogLevel.Warning);

#if ANDROID
            a.AddProvider(new AndroidLoggerProvider())
             .AddFilter("Microsoft", LogLevel.Warning)
             .AddFilter("Pekspro.RadioStorm", LogLevel.Information);
#endif

        });
#endif

        SQLitePCL.Batteries_V2.Init();
        var app = builder.Build();

        Task.Run(async () =>
        {
            //await Task.Delay(2000);
            await SetupAsync(app.Services);
            //var bootstrap = app.Services.GetRequiredService<Bootstrap.Bootstrap>();
            //await bootstrap.SetupAsync();
        }).Wait();
        
        return app;
    }
    
    public static async Task SetupAsync(IServiceProvider serviceProvider)
    {
        var bootstrap = serviceProvider.GetRequiredService<Bootstrap.Bootstrap>();
        await bootstrap.SetupAsync();

        var fileProvidersHelper = serviceProvider.GetRequiredService<IFileProvidersManager>();
        fileProvidersHelper.InitWithDelay();
    }
}
