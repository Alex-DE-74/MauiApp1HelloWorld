using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace KidJumpUp;

public static class HyperOsOpenAi
{
#if ANDROID

public static bool HatHintergrundFensterRecht()
{
    var context = Android.App.Application.Context;
    
    // 1. Geräte-Check (Xiaomi, Redmi, Poco)
    string hersteller = Build.Manufacturer?.ToLowerInvariant() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");
    if (!istXiaomi) return true; 

    try
    {
        var appOpsManager = (AppOpsManager)context.GetSystemService(Context.AppOpsService);
        if (appOpsManager == null) return true;

        int uid = context.ApplicationInfo.Uid;
        string packageName = context.PackageName;

        // 2. Offizieller, moderner Weg über den von Xiaomi genutzten API-String
        // "android:background_start_activity" entspricht exakt dem alten Int-Wert 10021
        const string opString = "android:background_start_activity";

        // Das ist ein offizielles SDK-Interface. Es benötigt KEINE Reflection 
        // und wird von Android 14 nicht blockiert.
        var mode = appOpsManager.CheckOpNoThrow(opString, uid, packageName);

        // Mode 0 = Allowed (Erlaubt)
        // Mode 1 = Ignored (Blockiert)
        // Mode 2 = Errored (Blockiert)
        return mode == AppOpsManagerMode.Allowed;
    }
    catch (Exception)
    {
        // Falls auf sehr alten Geräten der String nicht existiert, sicheres Fallback auf false,
        // damit wir im Zweifel lieber das Einstellungsmenü öffnen, statt blind true zu liefern.
        return false; 
    }
}

/// <summary>
/// Prüft das Xiaomi-spezifische Recht 'Hintergrundfenster öffnen'.
/// </summary>
public static bool HatHintergrundFensterRecht_v1()
{
    var context = Android.App.Application.Context;
    
    string hersteller = Build.Manufacturer?.ToLowerInvariant() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");
    if (!istXiaomi) return true; 

    try
    {
        var appOpsManager = (AppOpsManager)context.GetSystemService(Context.AppOpsService);
        if (appOpsManager == null) return true;

        // OP_BACKGROUND_START_ACTIVITY ist bei Xiaomi/HyperOS fix 10021
        int opValue = 10021; 

        // FEHLERBEHEBUNG 1: Nutzung der korrekten primitiven Java-Klassentypen
        var javaIntType = Java.Lang.Integer.Type;
        var javaStringType = Java.Lang.Class.FromType(typeof(Java.Lang.String));

        // FEHLERBEHEBUNG 2: 'noteOpNoThrow' ist auf neueren Systemen oft stabiler als 'checkOpNoThrow'
        var method = appOpsManager.Class.GetMethod("noteOpNoThrow", 
            javaIntType, 
            javaIntType, 
            javaStringType);

        int uid = context.ApplicationInfo.Uid;
        string packageName = context.PackageName;

        // Parameter als Java-Objekte übergeben
        var result = method.Invoke(appOpsManager, 
            IntPtr.Zero, // Für die params-Erweiterung im Bindungsprozess
            new Java.Lang.Object[] { 
                Java.Lang.Integer.ValueOf(opValue), 
                Java.Lang.Integer.ValueOf(uid), 
                new Java.Lang.String(packageName) 
            });

        if (result == null) return true;
        
        int mode = (int)result;

        // Mode 0 = AppOpsManager.ModeAllowed
        return mode == 0; 
    }
    catch (Exception ex)
    {
        // Prüfen Sie Ihr Logcat! Hier sehen Sie genau, warum es fehlschlug.
        Android.Util.Log.Error("HyperOS_Check", $"Fehler bei AppOps-Reflection: {ex}");
        return true; 
    }
}

    
    private const string PreferenceKey = "HyperOsAutostartHinweisGezeigt";

    /// <summary>
    /// Liest den echten Systemstatus von Xiaomis Recht 'Hintergrundfenster öffnen' via Reflection aus.
    /// Gibt TRUE zurück, wenn das Recht erlaubt ist (oder es kein Xiaomi ist).
    /// </summary>
    public static bool HatHintergrundFensterRecht_vO()
    {
        var context = Android.App.Application.Context;
        
        // Wenn es kein Xiaomi ist, blockiert uns das Recht nicht -> true
        string hersteller = Build.Manufacturer?.ToLowerInvariant() ?? "";
        bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");
        if (!istXiaomi) return true; 

        try
        {
            var appOpsManager = (AppOpsManager)context.GetSystemService(Context.AppOpsService);
            if (appOpsManager == null) return true;

            // 1. Holen des versteckten Integers für die Hintergrund-Aktivität (Wert ist fix 10021)
            int opValue = 10021; 

            // 2. Suchen der echten, versteckten numerischen Methode im Android-System
            // Wir müssen explizit nach (int, int, String) suchen, da Java das so deklariert
            var method = appOpsManager.Class.GetMethod("checkOpNoThrow", 
                Java.Lang.Class.FromType(typeof(int)), 
                Java.Lang.Class.FromType(typeof(int)), 
                Java.Lang.Class.FromType(typeof(string)));

            int uid = context.ApplicationInfo.Uid;
            string packageName = context.PackageName;

            // 3. Methode ausführen
            var result = method.Invoke(appOpsManager, opValue, uid, packageName);
            int mode = (int)result;

            // Mode 0 bedeutet in Android: MODE_ALLOWED (Erlaubt!)
            // Jede andere Zahl (1 = Ignored, 2 = Errored) bedeutet: Blockiert!
            return mode == 0; 
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("HyperOS", $"Status-Auslesen fehlgeschlagen, nutze Fallback: {ex.Message}");
            // Falls eine zukünftige HyperOS-Version die Methode komplett löscht, 
            // geben wir true zurück, um keine Endlosschleife zu riskieren.
            return true; 
        }
    }


// KORREKTUR: Task<bool> statt Task
public static async Task<bool> PruefeUndOeffneEchtenHyperOsAutostartAsync()
{
#if ANDROID
    // 1. HERSTELLER-CHECK
    string hersteller = Android.OS.Build.Manufacturer?.ToLower() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

    if (!istXiaomi) return false; // Kein Xiaomi? Code darf sofort weiterlaufen!

    // 2. SETTINGS-CHECK
    // Zum Testen auskommentieren
    bool wurdeBereitsGezeigt = Microsoft.Maui.Storage.Preferences.Default.Get(PreferenceKey, false);
    if (wurdeBereitsGezeigt) return false;

    bool bestaetigt = false;

    // 3. DIALOG ANZEIGEN
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
        if (page != null)
        {
            bestaetigt = await page.DisplayAlert(
                "HyperOS Hintergrund-Autostart",
                "Damit deine Wecker auch im Hintergrund zuverlässig funktionieren, "
                + "muss für diese App der Hintergrund-Autostart erlaubt werden.\n\n"
                + "HyperOS öffnet jetzt direkt die entsprechende Einstellung.",
                "Einstellungen öffnen",
                "Später");
        }
    });

    if (!bestaetigt)
        return false; // Nutzer klickt "Später" -> Weiterspringen ohne Block

    Preferences.Default.Set(PreferenceKey, true);

    OeffneHyperOsAutostart();
    return true; // WICHTIG: Signalisiert, dass das Menü offen ist und gewartet werden muss!
#else
    return false;
#endif
}


    private static bool IstXiaomi()
    {
        string manufacturer = (Build.Manufacturer ?? string.Empty).ToLowerInvariant();
        string brand = (Build.Brand ?? string.Empty).ToLowerInvariant();

        return manufacturer.Contains("xiaomi")
            || manufacturer.Contains("redmi")
            || manufacturer.Contains("poco")
            || brand.Contains("xiaomi")
            || brand.Contains("redmi")
            || brand.Contains("poco");
    }

    private static void OeffneHyperOsAutostart()
    {
        var context = Android.App.Application.Context;
        
        try
        {
            var intent = new Intent("miui.intent.action.OP_AUTO_START");
            intent.SetPackage("com.miui.securitycenter");
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "HyperOS",
                $"Der primäre Autostart-Weg ist fehlgeschlagen: {ex.Message}");
        }
    }

    public static void OeffneAppInfo()
    {
        var context = Android.App.Application.Context;
        
        try
        {
            var intent = new Intent(Settings.ActionApplicationDetailsSettings);
            intent.SetData(Android.Net.Uri.Parse("package:" + context.PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "HyperOS",
                $"App-Info-Seite konnte nicht geöffnet werden: {ex.Message}");
        }
    }

#else
    public static Task PruefeUndOeffneEchtenHyperOsAutostartAsync()
        => Task.CompletedTask;

    public static void OeffneAppInfo() 
        { }
#endif
}


/*
using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MauiApp1HelloWorld;

public static class HyperOsOpenAi
{
#if ANDROID

    private const string PreferenceKey = "HyperOsAutostartHinweisGezeigt";

    public static async Task PruefeUndOeffneEchtenHyperOsAutostartAsync()
    {
        if (!IstXiaomi())
            return;

        // Settings-Check (Zum Testen auskommentiert)
        // if (Preferences.Default.Get(PreferenceKey, false))
        //     return;

        bool bestaetigt = false;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = Application.Current?.MainPage;
            if (page == null)
                return;

            bestaetigt = await page.DisplayAlert(
                "HyperOS Hintergrund-Autostart",
                "Damit deine Wecker auch im Hintergrund zuverlässig funktionieren, "
                + "muss für diese App der Hintergrund-Autostart erlaubt werden.\n\n"
                + "HyperOS öffnet jetzt direkt die entsprechende Einstellung.",
                "Einstellungen öffnen",
                "Später");
        });

        if (!bestaetigt)
            return;

        Preferences.Default.Set(PreferenceKey, true);

        OeffneHyperOsAutostart();
    }

    private static bool IstXiaomi()
    {
        string manufacturer = (Build.Manufacturer ?? string.Empty).ToLowerInvariant();
        string brand = (Build.Brand ?? string.Empty).ToLowerInvariant();

        return manufacturer.Contains("xiaomi")
            || manufacturer.Contains("redmi")
            || manufacturer.Contains("poco")
            || brand.Contains("xiaomi")
            || brand.Contains("redmi")
            || brand.Contains("poco");
    }

    private static void OeffneHyperOsAutostart()
    {
        var context = Application.Context;
        
        try
        {
            var intent = new Intent("miui.intent.action.OP_AUTO_START");
            intent.SetPackage("com.miui.securitycenter");
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "HyperOS",
                $"Der primäre Autostart-Weg ist fehlgeschlagen: {ex.Message}");
        }
    }

    /// <summary>
    /// Öffnet die standardmäßige Android App-Info-Seite für diese Anwendung.
    /// Nützlich für Cache-Resets, Berechtigungen oder Akku-Optimierungseinstellungen.
    /// </summary>
    public static void OeffneAppInfo()
    {
        var context = Application.Context;
        
        try
        {
            var intent = new Intent(Settings.ActionApplicationDetailsSettings);
            intent.SetData(Android.Net.Uri.Parse("package:" + context.PackageName));
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "HyperOS",
                $"App-Info-Seite konnte nicht geöffnet werden: {ex.Message}");
        }
    }

#else
    public static Task PruefeUndOeffneAutostartAsync()
        => Task.CompletedTask;

    public static void OeffneAppInfo() 
        { }
#endif
}
*/
