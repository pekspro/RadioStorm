using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Pekspro.RadioStorm.Bootstrap;
using Pekspro.RadioStorm.Sandbox.WPF.LogFile;
using Pekspro.RadioStorm.Sandbox.WPF.Player;
using Pekspro.RadioStorm.Settings.SynchronizedSettings.FileProvider;

namespace Pekspro.RadioStorm.Sandbox.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    public static IServiceProvider ServiceProvider;

    private readonly IHost host;

    public App()
    {
        host = Host.CreateDefaultBuilder()
           .ConfigureServices((context, services) =>
           {
               ConfigureServices(context.Configuration, services);
           })
           .Build();

        SQLitePCL.Batteries.Init();
    }

    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        services.TryAddSingleton<IAudioManager, WpfAudioManager>();
        services.TryAddSingleton<IMainThreadRunner, MainThreadRunner>();
        services.TryAddSingleton<IVersionProvider, VersionProvider>();
        services.AddRadioStorm(configuration);
        services.AddRadioStormSandboxTools(configuration);
        services.AddRadioStormFileProviders(configuration, true);
        services.AddLogging(j => j
            .AddInMemory()
            .AddFileIfEnabled()
            // Change this in .csproj: <OutputType>Exe</OutputType>
            .AddConsole()
        );
        services.AddRadioStormUI(configuration);

        services.TryAddTransient<MainWindow>();
        services.TryAddTransient<ChannelDetailsWindow>();
        services.TryAddTransient<ChannelsWindow>();
        services.TryAddTransient<FavoritesWindow>();
        services.TryAddTransient<ProgramDetailsWindow>();
        services.TryAddTransient<ProgramSettingsWindow>();
        services.TryAddTransient<ProgramsWindow>();
        services.TryAddTransient<PlaylistWindow>();
        services.TryAddTransient<DownloadsWindow>();
        services.TryAddTransient<RecentEpisodesWindow>();
        services.TryAddTransient<SettingsWindow>();
        services.TryAddTransient<LogFileDetailsWindow>();
        services.TryAddTransient<EpisodeDetailsWindow>();
        services.TryAddTransient<LoggingWindow>();
        services.TryAddTransient<BackgroundTasksWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        ServiceProvider = host.Services;

        await host.StartAsync();

        RadioStormSandboxToolExtensions.SetupFakeRoamingFileProviders(host.Services);

        var bootstrap = host.Services.GetRequiredService<Bootstrap.Bootstrap>();
        await bootstrap.SetupAsync();

        var fileProvidersManager = host.Services.GetRequiredService<IFileProvidersManager>();
        fileProvidersManager.InitWithDelay();

        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        //var epsisodeWindow = host.Services.GetRequiredService<EpisodeWindow>();
        //epsisodeWindow.Show();




        //var channelsWindow = host.Services.GetRequiredService<ChannelsWindow>();
        //channelsWindow.Show();

        //var settingsService = host.Services.GetRequiredService<ISettingsService>();
        //settingsService.SetValue("Hello", "world");

        //var channelWindow = host.Services.GetRequiredService<ChannelWindow>();
        //channelWindow.Show();

        //var channelWindow2 = host.Services.GetRequiredService<ChannelWindow>();
        //channelWindow2.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        var shutDownManager = host.Services.GetRequiredService<IShutDownManager>();
        await shutDownManager.ShutDownAsync();

        using (host)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}
