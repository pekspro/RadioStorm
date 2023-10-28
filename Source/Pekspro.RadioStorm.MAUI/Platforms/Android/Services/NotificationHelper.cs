using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using AndroidX.Core.App;
using static Android.App.Notification;
using AndroidMedia = Android.Media;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

#nullable disable

public static class NotificationHelper
{
    public static readonly string CHANNEL_ID = "radiostorm_player_notification";
    private const int NotificationId = 1000;

    internal static Notification.Action GenerateActionCompat(Context context, int iconId, string title, string intentAction)
    {
        Intent intent = new Intent(context, typeof(MediaPlayerService));
        intent.SetAction(intentAction);

        PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
        if (intentAction.Equals(MediaPlayerService.ActionStop))
        {
            flags = PendingIntentFlags.CancelCurrent;
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            flags |= PendingIntentFlags.Mutable;
        }

        PendingIntent pendingIntent = PendingIntent.GetService(context, 1, intent, flags);

        Icon icon = Icon.CreateWithResource(context, iconId);
        
        return new Notification.Action.Builder(icon, title, pendingIntent).Build();
    }

    internal static void StopNotification(Context context)
    {
        NotificationManagerCompat nm = NotificationManagerCompat.From(context);
        nm.CancelAll();
    }

    internal static void CreateNotificationChannel(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            // Notification channels are new in API 26 (and not a part of the
            // support library). There is no need to create a notification
            // channel on older versions of Android.
            return;
        }

        var name = Strings.Services_Player_Name;
        var description = Strings.Services_Player_Description;
        var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Low)
        {
            Description = description
        };

        var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
        notificationManager.CreateNotificationChannel(channel);
    }

    internal static void StartNotification(
        Context context,
        MediaMetadata currentTrack,
        AndroidMedia.Session.MediaSession mediaSession,
        Bitmap largeIcon,
        bool isPlaying,
        PlayList playList,
        Service service)
    {
        
        if (currentTrack is null)
        {
            return;
        }

        var pendingIntent = PendingIntent.GetActivity(
            context,
            0,
            new Intent(context, typeof(MainActivity)),
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        MediaStyle style = new MediaStyle();
        style.SetMediaSession(mediaSession.SessionToken);

        int color = context.GetColor(_Microsoft.Android.Resource.Designer.ResourceConstant.Color.notificationBackground);

        var builder = new Builder(context, CHANNEL_ID)
            .SetStyle(style)
            .SetContentTitle(currentTrack.GetString(MediaMetadata.MetadataKeyTitle))
            .SetContentText(currentTrack.GetString(MediaMetadata.MetadataKeyArtist))
            .SetSubText(currentTrack.GetString(MediaMetadata.MetadataKeyAlbum))
            .SetColor(color)
            .SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_statusbar_play)
            .SetLargeIcon(largeIcon)
            .SetContentIntent(pendingIntent)
            .SetShowWhen(false)
            .SetOngoing(isPlaying)
            .SetVisibility(NotificationVisibility.Public);

        if (playList.IsLiveAudio)
        {
            AddPlayPauseActionCompat(builder, context, isPlaying);
            style.SetShowActionsInCompactView(0);
        }
        else
        {
            int showOffset = 0;

            if (playList.CanGoToPrevious)
            {
                builder.AddAction(GenerateActionCompat(context, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification_skip_previous, Strings.Player_Previous, MediaPlayerService.ActionPrevious));
                showOffset++;
            }
            
            builder.AddAction(GenerateActionCompat(context, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification_fast_rewind, Strings.Player_Backward, MediaPlayerService.ActionRewind));
            AddPlayPauseActionCompat(builder, context, isPlaying);
            builder.AddAction(GenerateActionCompat(context, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification_fast_forward, Strings.Player_Forward, MediaPlayerService.ActionForward));

            if (playList.CanGoToNext)
            {
                builder.AddAction(GenerateActionCompat(context, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification_skip_next, Strings.Player_Next, MediaPlayerService.ActionNext));
            }
            style.SetShowActionsInCompactView(showOffset + 0, showOffset + 1, showOffset + 2);
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            service.StartForeground(NotificationId, builder.Build(), ForegroundService.TypeMediaPlayback);
        }
        else
        {
            service.StartForeground(NotificationId, builder.Build());
        }

        //NotificationManagerCompat.From(context).Notify(NotificationId, builder.Build());
    }
    private static void AddPlayPauseActionCompat(
    Notification.Builder builder,
    Context context,
        bool isPlaying)
    {
        if (isPlaying)
        {
            builder.AddAction(GenerateActionCompat(context, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification_pause, Strings.Player_Pause, MediaPlayerService.ActionPause));
        }
        else
        {
            builder.AddAction(GenerateActionCompat(context, _Microsoft.Android.Resource.Designer.ResourceConstant.Drawable.ic_notification_play, Strings.Player_Play, MediaPlayerService.ActionPlay));
        }
    }
}
