using Foundation;

namespace Pekspro.RadioStorm.MAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override async bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        // Specify the Keychain access group
        App.iOSKeychainSecurityGroup = NSBundle.MainBundle.BundleIdentifier;

        await MauiProgram.SetupAsync();

        return base.FinishedLaunching(app, options);
    }

    // <OpenUrlSnippet>
    // Handling redirect URL
    // See: https://docs.microsoft.com/azure/active-directory/develop/msal-net-xamarin-ios-considerations
    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
        return true;
    }
}
