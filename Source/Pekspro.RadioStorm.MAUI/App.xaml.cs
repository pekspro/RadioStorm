using Application = Microsoft.Maui.Controls.Application;

namespace Pekspro.RadioStorm.MAUI;

public partial class App : Application
{
    private ILogger Logger { get; }

    private ILocalSettings LocalSettings { get; }

    public App()
    {
        InitializeComponent();

        Logger = Services.ServiceProvider.GetRequiredService<ILogger<App>>();
        Logger.LogInformation("Creating app.");

        MainPage = new AppShell();

        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(ChannelsPage), typeof(ChannelsPage));
        Routing.RegisterRoute(nameof(ChannelDetailsPage), typeof(ChannelDetailsPage));
        Routing.RegisterRoute(nameof(ProgramsPage), typeof(ProgramsPage));
        Routing.RegisterRoute(nameof(ProgramDetailsPage), typeof(ProgramDetailsPage));
        Routing.RegisterRoute(nameof(ProgramSettingsPage), typeof(ProgramSettingsPage));
        Routing.RegisterRoute(nameof(EpisodeDetailsPage), typeof(EpisodeDetailsPage));
        Routing.RegisterRoute(nameof(PlaylistPage), typeof(PlaylistPage));
        Routing.RegisterRoute(nameof(LogFileDetailsPage), typeof(LogFileDetailsPage));

        LocalSettings = Services.ServiceProvider.GetRequiredService<ILocalSettings>();
        var messenger = Services.ServiceProvider.GetRequiredService<IMessenger>();
        
        messenger.Register<SettingChangedMessage>(this, (r, m) =>
        {
            if (m.SettingName == nameof(LocalSettings.Theme))
            {
                UpdateTheme();
            }
        });

        UpdateTheme();
    }

    private void UpdateTheme()
    {
        var theme = LocalSettings.Theme;

        UserAppTheme = theme switch
        {
            ThemeType.Light => AppTheme.Light,
            ThemeType.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }

    public async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

#if WINDOWS
    protected async override void OnSleep()
    {
        base.OnSleep();

        Logger.LogInformation("OnSleep. Will shut down services, running on Windows.");
        
        var shutDownManager = Services.ServiceProvider.GetService<IShutDownManager>();

        if (shutDownManager is not null)
        {
            await shutDownManager.ShutDownAsync();
        }
    }
#else
    protected override void OnSleep()
    {
        base.OnSleep();

        Logger.LogInformation("OnSleep.");
    }
#endif

    protected override void OnStart()
    {
        base.OnStart();

        Logger.LogInformation("OnStart.");
    }

    protected override void OnResume()
    {
        base.OnResume();

        Logger.LogInformation("OnResume.");
    }
}
