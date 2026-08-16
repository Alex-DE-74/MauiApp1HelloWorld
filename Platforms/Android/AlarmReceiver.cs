using Android.Content;
using Android.App;

namespace KidJumpUp;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{   
    private readonly NotificationService _nService = new NotificationService();
    public override void OnReceive(Context context, Intent intent)
    {
        var prefs = context.GetSharedPreferences("alarm", FileCreationMode.Private);

        prefs.Edit()
        .PutLong("lastAlarm", Java.Lang.JavaSystem.CurrentTimeMillis())
        .Commit();
        
        // Der Aufruf bleibt kurz und knackig
        _nService.ZeigeKritischeNotificationVx(context);

        //SysAlert0(context);
        
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
}    
