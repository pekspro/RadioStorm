﻿using Microsoft.UI.Xaml;

#nullable enable

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pekspro.RadioStorm.MAUI.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application sealed class.
/// </summary>
public sealed partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        UnhandledException += App_UnhandledException;
    }

    private ILogger? _Logger;
    
    private ILogger Logger
    {
        get
        {
            return _Logger ??= ServiceProviderHelper.GetRequiredService<ILogger<App>>();
        }       
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Logger.LogError(e.Exception, "Unhandled exception.");
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        Logger.LogInformation("Application launched.");
    }
}
