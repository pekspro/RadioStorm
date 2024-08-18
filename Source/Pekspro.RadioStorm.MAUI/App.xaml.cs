using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.PlatformConfiguration.AndroidSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace Pekspro.RadioStorm.MAUI;

public sealed partial class App : Application
{
    private ILogger Logger { get; }

    private ILocalSettings LocalSettings { get; }

    private Microsoft.Maui.Handlers.IToolbarHandler? ToolbarHandler { get; set; }
    
    public App()
    {
        InitializeComponent();

        Logger = ServiceProviderHelper.GetRequiredService<ILogger<App>>();
        Logger.LogInformation("Creating app.");

        Microsoft.Maui.Handlers.ToolbarHandler.Mapper.AppendToMapping("mytoolbar", (handler, view) =>
        {
            ToolbarHandler = handler;
            SetupMenuBarColorFix();
        });

        RequestedThemeChanged += (s, e) => SetupMenuBarColorFix();
        
        MainPage = new AppShell();

        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        Routing.RegisterRoute(nameof(ChannelsPage), typeof(ChannelsPage));
        Routing.RegisterRoute(nameof(ChannelDetailsPage), typeof(ChannelDetailsPage));
        Routing.RegisterRoute(nameof(ScheduledEpisodesPage), typeof(ScheduledEpisodesPage));
        Routing.RegisterRoute(nameof(ChannelSongListPage), typeof(ChannelSongListPage));
        Routing.RegisterRoute(nameof(ProgramsPage), typeof(ProgramsPage));
        Routing.RegisterRoute(nameof(ProgramDetailsPage), typeof(ProgramDetailsPage));
        Routing.RegisterRoute(nameof(ProgramSettingsPage), typeof(ProgramSettingsPage));
        Routing.RegisterRoute(nameof(EpisodeDetailsPage), typeof(EpisodeDetailsPage));
        Routing.RegisterRoute(nameof(EpisodeSongListPage), typeof(EpisodeSongListPage));
        Routing.RegisterRoute(nameof(CurrentPlayingPage), typeof(CurrentPlayingPage));
        Routing.RegisterRoute(nameof(PlaylistPage), typeof(PlaylistPage));
        Routing.RegisterRoute(nameof(LogFileDetailsPage), typeof(LogFileDetailsPage));

        LocalSettings = ServiceProviderHelper.GetRequiredService<ILocalSettings>();
        var messenger = ServiceProviderHelper.GetRequiredService<IMessenger>();
        
        messenger.Register<SettingChangedMessage>(this, (r, m) =>
        {
            if (m.SettingName == nameof(LocalSettings.Theme))
            {
                UpdateTheme();
            }
        });

        UpdateTheme();
    }

    public void SetupMenuBarColorFix()
    {
        if (ToolbarHandler is null)
        {
            return;
        }

#if ANDROID
        try
        {
            if (RequestedTheme == AppTheme.Light)
            {
                ToolbarHandler.PlatformView.OverflowIcon = Platform.CurrentActivity!.GetDrawable(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_more_vert_24_light);
            }
            else
            {
                ToolbarHandler.PlatformView.OverflowIcon = Platform.CurrentActivity!.GetDrawable(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_more_vert_24_dark);
            }
        }
        catch (Exception ex)
        {
            /* We have this stack trace that indicates something bad could happen here:
             * 
                Exception android.runtime.JavaProxyThrowable: System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation.
                ---> System.InvalidOperationException: PlatformView cannot be null here
                at Microsoft.Maui.Handlers.ElementHandler`2[[Microsoft.Maui.IToolbar, Microsoft.Maui, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null],[Google.Android.Material.AppBar.MaterialToolbar, Xamarin.Google.Android.Material, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].get_PlatformView()
                at Microsoft.Maui.Handlers.ToolbarHandler.Microsoft.Maui.Handlers.IToolbarHandler.get_PlatformView()
                at Pekspro.RadioStorm.MAUI.App.SetupMenuBarColorFix()
                at Pekspro.RadioStorm.MAUI.App.<.ctor>b__10_1(Object s, AppThemeChangedEventArgs e)
                at System.Reflection.MethodInvoker.InterpretedInvoke(Object obj, Span`1 args, BindingFlags invokeAttr)
                --- End of inner exception stack trace ---
                at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
                at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
                at Microsoft.Maui.WeakEventManager.HandleEvent(Object sender, Object args, String eventName)
                at Microsoft.Maui.Controls.Application.TriggerThemeChangedActual()
                at Microsoft.Maui.Controls.Application.set_UserAppTheme(AppTheme value)
                at Pekspro.RadioStorm.MAUI.App.UpdateTheme()
                at Pekspro.RadioStorm.MAUI.App.<.ctor>b__10_2(Object r, SettingChangedMessage m)
                at CommunityToolkit.Mvvm.Messaging.Internals.MessageHandlerDispatcher.For`2[[System.Object, System.Private.CoreLib, Version=7.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[Pekspro.RadioStorm.Settings.SettingChangedMessage, Pekspro.RadioStorm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].Invoke(Object recipient, Object message)
                at CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.SendAll[SettingChangedMessage](ReadOnlySpan`1 pairs, Int32 i, SettingChangedMessage message)
                at CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Send[SettingChangedMessage,Unit](SettingChangedMessage message, Unit token)
                at CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.Send[SettingChangedMessage](IMessenger messenger, SettingChangedMessage message)
                at Pekspro.RadioStorm.Settings.LocalSettings.NotifySettingChanged(String settingsName)
                at Pekspro.RadioStorm.Settings.LocalSettings.set_Theme(ThemeType value)
                at Pekspro.RadioStorm.UI.ViewModel.Settings.SettingsViewModel.set_ThemeIndex(Int32 value)           
            */
        
            Logger.LogError(ex, "Failed to setup menu bar color fix.");
            
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
#endif
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

    public void ConfigureStatusBar()
    {
#if ANDROID
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var act = Platform.CurrentActivity;

            if (act is null)
            {
                return;
            }
            
            if (RequestedTheme == AppTheme.Light)
            {
                CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.DarkContent);

                CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Color.FromRgb(255, 255, 255));

                if (act?.Window is not null)
                {
                    Shell.Current.CurrentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetStyle(NavigationBarStyle.DarkContent);
            
                    act.Window.SetNavigationBarColor(Android.Graphics.Color.Rgb(255, 255, 255));
                }
            }
            else
            {
                CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);

                CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Color.FromRgb(0, 0, 0));

                if (act?.Window is not null)
                {
                    Shell.Current.CurrentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetStyle(NavigationBarStyle.LightContent);

                    act.Window.SetNavigationBarColor(Android.Graphics.Color.Rgb(0, 0, 0));
                }
            }
        });
#endif

        }


        public async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    #if WINDOWS
    protected override void OnSleep()
    {
        base.OnSleep();

        //Logger.LogInformation("OnSleep. Will shut down services, running on Windows.");

        //var shutDownManager = ServiceProviderHelper.GetService<IShutDownManager>();

        //if (shutDownManager is not null)
        //{
        //    await shutDownManager.ShutDownAsync();
        //}
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
