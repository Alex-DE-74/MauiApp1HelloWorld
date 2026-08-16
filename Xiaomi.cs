/*
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;

#if ANDROID

using Android.App;
using Android.Content;

#endif
*/
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

using Android.Content;
using Android.OS;
using AndroidX.Core.Content;

namespace KidJumpUp;

public static class Xiaomi
{
	
#if ANDROID
	
public static async Task PruefeUndOeffneAutostartWennNoetigAsync()
{
    var context = Android.App.Application.Context;

    // 1. HERSTELLER-CHECK: Nur auf Xiaomi, Redmi und POCO ausführen
    string hersteller = Build.Manufacturer?.ToLower() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

    if (!istXiaomi) return; // Kein Xiaomi? Sofort beenden.

    // 2. STATUS-CHECK (Verfügbar ab Android 11 / API 30)
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            var future = PackageManagerCompat.GetUnusedAppRestrictionsStatus(context);
            
            // Holt das Ergebnis aus dem Java-Future (Zahlenwert 1-4)
            int status = (int)await Task.Run(() => future.Get());

            // DIE KORREKTE GOOGLE-LOGIK:
            // Status 1 = DISABLED (Der Nutzer hat den Schalter bereits deaktiviert -> Alles sicher!)
            // Status >= 2 = RESTRICTED (Der Schalter ist AKTIV und blockiert deine App!)
            if (status >= 2) 
            {
                // Wechselt sauber auf den MAUI UI-Thread für den Dialog
                await Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(async () =>
                {
                    bool userKlick = await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "HyperOS Optimierung",
                        "Bitte deaktiviere im nächsten Bildschirm die Option 'App-Aktivität bei Nichtbenutzung pausieren', damit deine Wecker im Hintergrund zuverlässig funktionieren.",
                        "Zu den Einstellungen",
                        "Abbrechen");

                    if (userKlick)
                    {
                        ResolveHyperOsAutostartRestriction();
                    }
                });
            }
            else
            {
                // Status ist 1 (oder im Fehlerfall 0). Der Schalter ist bereits aus.
                System.Diagnostics.Debug.WriteLine("App ist bereits vor der Hibernation geschützt.");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler bei der API-Abfrage: {ex.Message}");
        }
    }
}

private static void ResolveHyperOsAutostartRestriction()
{
    var context = Android.App.Application.Context;
    
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            // Erzeugt den systemkonformen Intent für ungenutzte Apps
            var intent = IntentCompat.CreateManageUnusedAppRestrictionsIntent(context, context.PackageName);
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (System.Exception)
        {
            // Fallback auf die direkte Xiaomi-Berechtigungsebene
            TryOpenXiaomiSecurityDirectly(context);
        }
    }
}

private static void TryOpenXiaomiSecurityDirectly(Context context)
{
    var intent = new Intent();
    intent.SetComponent(new ComponentName(
        "com.miui.securitycenter", 
        "com.miui.permcenter.autostart.AutoStartManagementActivity"));
    intent.AddFlags(ActivityFlags.NewTask);

    try
    {
        context.StartActivity(intent);
    }
    catch
    {
        try
        {
            var fallbackIntent = new Intent("miui.intent.action.OP_AUTO_START");
            fallbackIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(fallbackIntent);
        }
        catch
        {
            // Letzter Rettungsanker: Normale App-Details-Seite
            var appInfoIntent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
            var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
            appInfoIntent.SetData(uri);
            appInfoIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(appInfoIntent);
        }
    }
}

#endif
 
}
