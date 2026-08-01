using Android.Content;
using Android.App;

namespace MauiApp1HelloWorld;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, "alarm_channel_id")
    .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
    .SetContentTitle("DEBUG")
    .SetContentText("AlarmReceiver wurde gestartet")
    .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh);

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
        manager?.Notify(12345, builder.Build());
        
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
}
