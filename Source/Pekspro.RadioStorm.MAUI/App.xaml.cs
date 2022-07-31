using Application = Microsoft.Maui.Controls.Application;

namespace Pekspro.RadioStorm.MAUI;

public partial class App : Application
{
    private ILocalSettings LocalSettings;
    
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        // MainPage = new MainPage();
        // MainPage = new ChannelsPage();

        Routing.RegisterRoute("test", typeof(MainPage));
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
        
        var shutDownManager = Services.ServiceProvider.GetService<IShutDownManager>();

        if (shutDownManager is not null)
        {
            await shutDownManager.ShutDownAsync();
        }
    }
#endif
}
