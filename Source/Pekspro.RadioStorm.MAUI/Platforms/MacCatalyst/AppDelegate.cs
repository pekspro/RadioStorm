using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Pekspro.RadioStorm.MAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
