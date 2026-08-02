using Android.Content;
using Android.App;

namespace MauiApp1HelloWorld;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        // Der Aufruf bleibt kurz und knackig
        ZeigeKritischeNotification(context);
                
        // Toast       
        Android.Widget.Toast.MakeText(
        context,
        "AlarmReceiver gestartet",
        Android.Widget.ToastLength.Long).Show();

        // Wenn der Wecker klingelt, holen wir die MainPage in den Vordergrund
        Intent i = new Intent(context, typeof(MainActivity));
        i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        context.StartActivity(i);

        // Startet sofort die Shake-Challenge, sobald die App aufwacht
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Shell.Current?.CurrentPage is MainPage mainPage)
            {
                mainPage.StartShakeChallenge();
            }
        });
    }

private void ZeigeKritischeNotification(Android.Content.Context context)
{
    var channelId = "alarm_channel_id";
    var manager = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);

    // 1. Kanal auf das Maximum stellen (Android 8.0+)
    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
    {
        // NotificationImportance.High ist das native Maximum für Kanäle
        var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
        {
            LockscreenVisibility = Android.App.NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        channel.SetBypassDnd(true); // Umgeht den "Bitte nicht stören"-Modus
        manager?.CreateNotificationChannel(channel);
    }

    // 2. FullScreenIntent für das Aufwachen des schwarzen Bildschirms
    var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
    var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
        context, 
        99, 
        fullScreenIntent, 
        Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

    // 3. Builder auf absolute Höchststufe einstellen
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        
        // HIER WIRD AUF ABSOLUTES MAXIMUM GEWECHSELT:
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) // Max-Priorität erzwingt das sofortige Banner-Popup
        
        .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll) 
        .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
        .SetFullScreenIntent(fullScreenPendingIntent, true) 
        .SetAutoCancel(true);

    manager?.Notify(12345, builder.Build());
}

}
