using System.Threading.Tasks;

namespace MauiApp1HelloWorld;

// zeigt 
// App info
// Autostart 
public static class HyperOsOpenAi_V1
{
    public static async Task PruefeUndOeffneEchtenHyperOsAutostartAsync_v2()
    {
#if ANDROID
        // 1. HERSTELLER-CHECK: Nur auf Xiaomi, Redmi und POCO ausführen
        string hersteller = Android.OS.Build.Manufacturer?.ToLower() ?? "";
        bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

        if (!istXiaomi) return; // Kein Xiaomi? Sofort beenden.

        // 2. SETTINGS-CHECK: Verhindert, dass der Dialog bei jedem Weckerstellen nervt
        bool wurdeBereitsGezeigt = false; //Microsoft.Maui.Storage.Preferences.Default.Get("HyperOsAutostartGezeigt", false);
        if (wurdeBereitsGezeigt) return;

        // 3. DIALOG ANZEIGEN: Erzwingt den Sprung auf den MAUI Haupt-UI-Thread
        bool userKlick = false;
        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page != null)
            {
                userKlick = await Microsoft.Maui.Controls.Application.Current.Windows[0].Page.DisplayAlertAsync(
                    "HyperOS Hintergrund-Schutz",
                    "Damit deine Wecker nach dem Wegwischen der App zuverlässig funktionieren, aktiviere bitte im nächsten Bildschirm den Schalter 'Hintergrund-Autostart'.",
                    "Zu den Einstellungen",
                    "Abbrechen");
            }
        });

        // 4. BEI ZUSTIMMUNG: Direkt in die App-Info-Details springen
        
        if (userKlick)
{
    // Zustand persistent speichern
    Microsoft.Maui.Storage.Preferences.Default.Set("HyperOsAutostartGezeigt", true);

    var context = Android.App.Application.Context;
    
    try
    {
        // HAUPT-WEG für HyperOS / Android 14: Springt direkt in das "Berechtigungen"-Menü der Apps
        var intent1 = new Android.Content.Intent();
        intent1.SetComponent(new Android.Content.ComponentName(
            "com.miui.securitycenter", 
            "com.miui.permcenter.permissions.PermissionsMainActivity")); // Öffnet die Berechtigungs-Übersicht
        intent1.AddFlags(Android.Content.ActivityFlags.NewTask);
        context.StartActivity(intent1);
    }
    catch (System.Exception)
    {
        try
        {
            // FALLBACK für ältere/andere HyperOS-Builds: Direkt in die Autostart-Verwaltung
            var intent2 = new Android.Content.Intent("miui.intent.action.OP_AUTO_START");
            intent2.AddFlags(Android.Content.ActivityFlags.NewTask);
            context.StartActivity(intent2);
        }
        catch (System.Exception)
        {
            // LETZTER NOTANKER: Öffnet das übergeordnete App-Menü der Systemeinstellungen
            var intent3 = new Android.Content.Intent(Android.Provider.Settings.ActionManageApplicationsSettings);
            intent3.AddFlags(Android.Content.ActivityFlags.NewTask);
            context.StartActivity(intent3);
        }
    }
}

        if (userKlick)
        {
            // Zustand sofort persistent speichern
            Microsoft.Maui.Storage.Preferences.Default.Set("HyperOsAutostartGezeigt", true);

            var context = Android.App.Application.Context;
            
            try
            {
                // Dieser Aufruf ist vom Android-System geschützt und wird von HyperOS NIEMALS blockiert.
                // Er führt den Nutzer direkt auf die Einstellungsseite UNSERER eigenen App.
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
                intent.SetData(uri);
                intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WEKER-ERROR] Notanker fehlgeschlagen: {ex.Message}");
            }
        }
#else
        await Task.CompletedTask;
#endif
    }
    
    public static async Task PruefeUndOeffneEchtenHyperOsAutostartAsync_v1()
    {
#if ANDROID
        // 1. HERSTELLER-CHECK: Nur auf Xiaomi, Redmi und POCO ausführen
        string hersteller = Android.OS.Build.Manufacturer?.ToLower() ?? "";
        bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

        if (!istXiaomi) return; // Kein Xiaomi? Sofort abbrechen.

        // 2. SETTINGS-CHECK: Wurde das Menü in der Vergangenheit bereits aufgerufen?
        bool wurdeBereitsGezeigt = Microsoft.Maui.Storage.Preferences.Default.Get("HyperOsAutostartGezeigt", false);

        if (wurdeBereitsGezeigt)
        {
            return; // Verhindert das mehrfache Aufrufen
        }

        // 3. DIALOG ANZEIGEN: Erzwingt den Sprung auf den MAUI Haupt-UI-Thread [STEM]
        bool userKlick = false;
        await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Microsoft.Maui.Controls.Application.Current?.Windows[0].Page != null)
            {
                userKlick = await Microsoft.Maui.Controls.Application.Current.Windows[0].Page.DisplayAlertAsync(
                    "HyperOS Hintergrund-Schutz",
                    "Damit deine Wecker nach dem Wegwischen der App zuverlässig funktionieren, aktiviere bitte im nächsten Bildschirm den 'Hintergrund-Autostart' für diese App.",
                    "Zu den Einstellungen",
                    "Abbrechen");
            }
        });

        // 4. BEI ZUSTIMMUNG: Xiaomi-Sicherheitszentrum ansteuern
        if (userKlick)
        {
            // Zustand sofort in den lokalen App-Settings abspeichern
            Microsoft.Maui.Storage.Preferences.Default.Set("HyperOsAutostartGezeigt", true);

            var context = Android.App.Application.Context;
            
            // LÖSUNG: Wir versuchen nacheinander alle bekannten HyperOS-Wege mit der nötigen Kategorie
            try
            {
                // Weg 1: Der direkte Pfad über die Komponente
                var intent1 = new Android.Content.Intent();
                intent1.SetComponent(new Android.Content.ComponentName(
                    "com.miui.securitycenter", 
                    "com.miui.permcenter.autostart.AutoStartManagementActivity"));
                intent1.AddCategory(Android.Content.Intent.CategoryDefault); // ZWINGEND ERFORDERLICH!
                intent1.AddFlags(Android.Content.ActivityFlags.NewTask);
                context.StartActivity(intent1);
            }
            catch (System.Exception)
            {
                try
                {
                    // Weg 2: Der modernere HyperOS Fallback über die globale Action
                    var intent2 = new Android.Content.Intent("miui.intent.action.OP_AUTO_START");
                    intent2.AddCategory(Android.Content.Intent.CategoryDefault); // ZWINGEND ERFORDERLICH!
                    intent2.AddFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(intent2);
                }
                catch (System.Exception)
                {
                    // Weg 3: Absoluter Notanker -> Öffnet die App-Info-Seite, wo der Nutzer den Autostart findet
                    var intent3 = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                    var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
                    intent3.SetData(uri);
                    intent3.AddFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(intent3);
                }
            }
        }
#else
        // Für andere Plattformen
        await Task.CompletedTask;
#endif
    }
}
