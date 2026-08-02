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
        // 1. Intent für das Aufwecken des Bildschirms vorbereiten
        var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
            context, 
            99, 
            fullScreenIntent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        // 2. Die Systemnachricht mit allen Rechten für Chrome & Sperrbildschirm bauen
        var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, "alarm_channel_id")
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh)
            .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
            .SetVisibility(AndroidX.Core.App.NotificationCompat.Builder.VisibilityPublic) 
            .SetFullScreenIntent(fullScreenPendingIntent, true) 
            .SetAutoCancel(true);

        // 3. Nachricht absenden
        var manager = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);
        manager?.Notify(12345, builder.Build());
    }    
}
