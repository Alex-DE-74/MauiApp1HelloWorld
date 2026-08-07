using System.Threading.Tasks;

namespace MauiApp1HelloWorld;

public static class HyperOs
{
    public static async Task PruefeUndOeffneEchtenHyperOsAutostartAsync()
    {
#if ANDROID
        // 1. HERSTELLER-CHECK: Nur auf Xiaomi, Redmi und POCO ausführen
        string hersteller = Android.OS.Build.Manufacturer?.ToLower() ?? "";
        bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

        if (!istXiaomi) return; // Kein Xiaomi? Sofort abbrechen.

        // 2. SETTINGS-CHECK: Wurde das Menü in der Vergangenheit bereits aufgerufen?
        // Nutzen der persistenten MAUI-Preferences
        bool wurdeBereitsGezeigt = Microsoft.Maui.Storage.Preferences.Default.Get("HyperOsAutostartGezeigt", false);

        if (wurdeBereitsGezeigt)
        {
            System.Diagnostics.Debug.WriteLine("[WEKER] Autostart-Einstellung wurde bereits früher aufgerufen.");
            return; 
        }

        // 3. DIALOG ANZEIGEN: Erzwingt den Sprung auf den MAUI Haupt-UI-Thread
        bool userKlick = false;
        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Wichtig: Da wir nicht mehr in der MainPage sind, rufen wir das Popup über Application.Current auf
            if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
            {
                userKlick = await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                    "HyperOS Hintergrund-Schutz",
                    "Damit deine Wecker nach dem Wegwischen der App zuverlässig funktionieren, aktiviere bitte im nächsten Bildschirm den 'Hintergrund-Autostart' für diese App.",
                    "Zu den Einstellungen",
                    "Abbrechen");
            }
        });

        // 4. BEI ZUSTIMMUNG: Xiaomi-Sicherheitszentrum ansteuern
        if (userKlick)
        {
            // Zustand in den lokalen App-Settings abspeichern
            Microsoft.Maui.Storage.Preferences.Default.Set("HyperOsAutostartGezeigt", true);

            var context = Android.App.Application.Context;
            var intent = new Android.Content.Intent();
            
            // Direkter Pfad zur Autostart-Verwaltung im Xiaomi Security-Center
            intent.SetComponent(new Android.Content.ComponentName(
                "com.miui.securitycenter", 
                "com.miui.permcenter.autostart.AutoStartManagementActivity"));
            intent.AddFlags(Android.Content.ActivityFlags.NewTask);

            try
            {
                context.StartActivity(intent);
            }
            catch (System.Exception)
            {
                try
                {
                    // Fallback 1: Falls die Activity in dieser HyperOS-Version verschoben wurde
                    var fallbackIntent = new Android.Content.Intent("miui.intent.action.OP_AUTO_START");
                    fallbackIntent.AddFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(fallbackIntent);
                }
                catch (System.Exception)
                {
                    // Fallback 2: Die allgemeine App-Info-Seite
                    var appInfoIntent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                    var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
                    appInfoIntent.SetData(uri);
                    appInfoIntent.AddFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(appInfoIntent);
                }
            }
        }
#else
        // Macht die Methode auf iOS, Windows etc. zu einer sicheren, leeren Operation
        await Task.CompletedTask;
#endif
    }
}
