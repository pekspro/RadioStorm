using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Pekspro.RadioStorm.UI.Resources;
using static Android.App.Notification;
using AndroidMedia = Android.Media;

namespace Pekspro.RadioStorm.MAUI.Platforms.Android.Services;

#nullable disable

public static class NotificationHelper
{
    public static readonly string CHANNEL_ID = "location_notification";
    private const int NotificationId = 1000;

    internal static Notification.Action GenerateActionCompat(Context context, int icon, string title, string intentAction)
    {
        Intent intent = new Intent(context, typeof(MediaPlayerService));
        intent.SetAction(intentAction);

        PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
        if (intentAction.Equals(MediaPlayerService.ActionStop))
        {
            flags = PendingIntentFlags.CancelCurrent;
        }

        flags |= PendingIntentFlags.Mutable;

        PendingIntent pendingIntent = PendingIntent.GetService(context, 1, intent, flags);

        return new Notification.Action.Builder(icon, title, pendingIntent).Build();
    }

    internal static void StopNotification(Context context)
    {
        NotificationManagerCompat nm = NotificationManagerCompat.From(context);
        nm.CancelAll();
    }

    internal static void CreateNotificationChannel(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            // Notification channels are new in API 26 (and not a part of the
            // support library). There is no need to create a notification
            // channel on older versions of Android.
            return;
        }

        var name = "Local Notifications";
        var description = "The count from MainActivity.";
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
        var pendingIntent = PendingIntent.GetActivity(
            context,
            0,
            new Intent(context, typeof(MainActivity)),
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable);
        
        if (currentTrack is null)
        {
            return;
        }

        MediaStyle style = new MediaStyle();
        style.SetMediaSession(mediaSession.SessionToken);

        int color = context.GetColor(Resource.Color.notificationBackground);

        var builder = new Builder(context, CHANNEL_ID)
            .SetStyle(style)
            .SetContentTitle(currentTrack.GetString(MediaMetadata.MetadataKeyTitle))
            .SetContentText(currentTrack.GetString(MediaMetadata.MetadataKeyArtist))
            .SetSubText(currentTrack.GetString(MediaMetadata.MetadataKeyAlbum))
            .SetColor(color)
            .SetSmallIcon(Resource.Drawable.ic_statusbar_play)
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
                builder.AddAction(GenerateActionCompat(context, Resource.Drawable.ic_notification_skip_previous, Strings.Player_Previous, MediaPlayerService.ActionPrevious));
                showOffset++;
            }
            
            builder.AddAction(GenerateActionCompat(context, Resource.Drawable.ic_notification_fast_rewind, Strings.Player_Backward, MediaPlayerService.ActionRewind));
            AddPlayPauseActionCompat(builder, context, isPlaying);
            builder.AddAction(GenerateActionCompat(context, Resource.Drawable.ic_notification_fast_forward, Strings.Player_Forward, MediaPlayerService.ActionForward));

            if (playList.CanGoToNext)
            {
                builder.AddAction(GenerateActionCompat(context, Resource.Drawable.ic_notification_skip_next, Strings.Player_Next, MediaPlayerService.ActionNext));
            }
            
            style.SetShowActionsInCompactView(showOffset + 0, showOffset + 1, showOffset + 2);
        }

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
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
            builder.AddAction(GenerateActionCompat(context, Resource.Drawable.ic_notification_pause, Strings.Player_Pause, MediaPlayerService.ActionPause));
        }
        else
        {
            builder.AddAction(GenerateActionCompat(context, Resource.Drawable.ic_notification_play, Strings.Player_Play, MediaPlayerService.ActionPlay));
        }
    }
}
