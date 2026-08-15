#if ANDROID
using Android.App;
using Android.Content;

namespace MauiApp1HelloWorld;

// KORREKTUR: Explizite Nutzung von 'IntentFilterAttribute', um den Konflikt zu lösen
[BroadcastReceiver(Enabled = true, Exported = false, DirectBootAware = true)]
[IntentFilterAttribute(new[] { Intent.ActionBootCompleted, "android.intent.action.QUICKBOOT_POWERON" })]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent.Action == Intent.ActionBootCompleted || intent.Action == "android.intent.action.QUICKBOOT_POWERON")
        {
            // 1. Hole die gespeicherte Weckerzeit aus den Preferences
            long triggerZeitpunktMs = Microsoft.Maui.Storage.Preferences.Default.Get("GeplanteWeckerZeitMs", 0L);
            
            long aktuelleZeitMs = Java.Lang.JavaSystem.CurrentTimeMillis();

            // 2. Prüfen, ob der Wecker überhaupt noch in der Zukunft liegt
            // if (triggerZeitpunktMs > aktuelleZeitMs)
            {
                long verbleibendeSekunden = (triggerZeitpunktMs - aktuelleZeitMs) / 1000;

                // 3. Wecker neu setzen (Ruft deine MainPage-Methode auf)
                var mainPage = new MainPage(); 
                mainPage.StarteExaktenWecker(context, verbleibendeSekunden);
            }
        }
    }
}
#endif
