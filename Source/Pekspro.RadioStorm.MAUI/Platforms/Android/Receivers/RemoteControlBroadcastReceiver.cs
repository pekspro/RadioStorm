using Android.App;
using Android.Content;
using Android.Views;
using Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Receivers;

#nullable disable

[BroadcastReceiver(Exported = true)]
[IntentFilter(new[] { Intent.ActionMediaButton })]
public sealed class RemoteControlBroadcastReceiver : BroadcastReceiver
{
    private ILogger _Logger;

    private ILogger Logger
    {
        get
        {
            return _Logger ??= MAUI.Services.ServiceProvider.Current.GetRequiredService<ILogger<RemoteControlBroadcastReceiver>>();
        }
    }


    /// <summary>
    /// gets the sealed class name for the component
    /// </summary>
    /// <value>The name of the component.</value>
    public string ComponentName { get { return this.Class.Name; } }

    /// <Docs>The Context in which the receiver is running.</Docs>
    /// <summary>
    /// When we receive the action media button intent
    /// parse the key event and tell our service what to do.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <param name="intent">Intent.</param>
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action != Intent.ActionMediaButton)
        {
            return;
        }

        //The event will fire twice, up and down.
        // we only want to handle the down event though.
        var key = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
        if (key.Action != KeyEventActions.Down)
        {
            return;
        }

        Logger.LogInformation("Key code: {keyCode}", key.KeyCode);

        string action;

        switch (key.KeyCode)
        {
            case Keycode.Headsethook:
            case Keycode.MediaPlayPause:
                action = MediaPlayerService.ActionTogglePlayback;
                break;
            case Keycode.MediaPlay:
                action = MediaPlayerService.ActionPlay;
                break;
            case Keycode.MediaPause:
                action = MediaPlayerService.ActionPause;
                break;
            case Keycode.MediaStop:
                action = MediaPlayerService.ActionStop;
                break;
            case Keycode.MediaRewind:
                action = MediaPlayerService.ActionRewind;
                break;
            case Keycode.MediaFastForward:
                action = MediaPlayerService.ActionForward;
                break;
            case Keycode.MediaNext:
                action = MediaPlayerService.ActionNext;
                break;
            case Keycode.MediaPrevious:
                action = MediaPlayerService.ActionPrevious;
                break;
            default:
                return;
        }

        Logger.LogInformation("Action: {action}", action);

        var remoteIntent = new Intent(action);
        context.StartService(remoteIntent);
    }
}
