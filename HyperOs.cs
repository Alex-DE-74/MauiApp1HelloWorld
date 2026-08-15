using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MauiApp1HelloWorld;

public static class HyperOs
{
#if ANDROID

    private const string PreferenceKey =
        "HyperOsAutostartHinweisGezeigt";

    public static async Task PruefeUndOeffneAutostartAsync()
    {
        // Nur Xiaomi / Redmi / POCO
        if (!IstXiaomi())
            return;

        // Hinweis nicht bei jedem Start anzeigen
        if (Preferences.Default.Get(PreferenceKey, false))
            return;

        bool bestaetigt = false;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = Application.Current?.Windows.Count > 0
                ? Application.Current.Windows[0].Page
                : null;

            if (page == null)
                return;

            bestaetigt = await page.DisplayAlertAsync(
                "HyperOS Hintergrund-Autostart",
                "Damit deine Wecker auch nach dem Schließen "
                + "der App zuverlässig funktionieren, sollte für "
                + "diese App der Hintergrund-Autostart aktiviert sein.\n\n"
                + "HyperOS versucht jetzt, die entsprechende "
                + "Einstellung zu öffnen.",
                "Einstellungen öffnen",
                "Später");
        });

        if (!bestaetigt)
            return;

        // Achtung:
        // Das bedeutet NICHT, dass Autostart aktiviert wurde.
        // Es bedeutet nur, dass der Hinweis angezeigt wurde.
        Preferences.Default.Set(PreferenceKey, true);

        OeffneXiaomiAutostart();
    }


    private static bool IstXiaomi()
    {
        string manufacturer =
            (Build.Manufacturer ?? string.Empty)
            .ToLowerInvariant();

        string brand =
            (Build.Brand ?? string.Empty)
            .ToLowerInvariant();

        return manufacturer.Contains("xiaomi")
            || manufacturer.Contains("redmi")
            || manufacturer.Contains("poco")
            || brand.Contains("xiaomi")
            || brand.Contains("redmi")
            || brand.Contains("poco");
    }


    private static void OeffneXiaomiAutostart()
    {
        var context = Android.App.Application.Context;
        var packageManager = context.PackageManager;

        /*
         * Xiaomi/HyperOS verwendet interne Activities.
         *
         * Es gibt dafür leider keine öffentliche Android-API.
         * Deshalb versuchen wir mehrere bekannte Xiaomi-Wege.
         *
         * Jeder Intent wird vorher mit ResolveActivity geprüft.
         */

        Intent[] intents =
        {
            // ---------------------------------------------------------
            // 1. Bekannte Xiaomi AutoStart Activity
            // ---------------------------------------------------------
            new Intent()
                .SetComponent(
                    new ComponentName(
                        "com.miui.securitycenter",
                        "com.miui.permcenter.autostart.AutoStartManagementActivity")),

            // ---------------------------------------------------------
            // 2. Xiaomi AutoStart Action
            // ---------------------------------------------------------
            new Intent(
                "miui.intent.action.OP_AUTO_START")
                .SetPackage("com.miui.securitycenter"),

            // ---------------------------------------------------------
            // 3. Xiaomi App-Permission-Editor
            // ---------------------------------------------------------
            new Intent(
                "miui.intent.action.APP_PERM_EDITOR")
                .SetPackage("com.miui.securitycenter")
                .PutExtra(
                    "extra_pkgname",
                    context.PackageName),

            // ---------------------------------------------------------
            // 4. App-Permission-Editor ohne Package-Beschränkung
            // ---------------------------------------------------------
            new Intent(
                "miui.intent.action.APP_PERM_EDITOR")
                .PutExtra(
                    "extra_pkgname",
                    context.PackageName)
        };


        foreach (var intent in intents)
        {
            try
            {
                intent.AddFlags(
                    ActivityFlags.NewTask);

                if (intent.ResolveActivity(packageManager) != null)
                {
                    context.StartActivity(intent);
                    return;
                }
            }
            catch (Android.Content.ActivityNotFoundException)
            {
                // Dieser Xiaomi-Weg existiert auf diesem Gerät nicht.
            }
            ///catch (Android.Content.SecurityException)
            //{
            ///    // Activity existiert, darf aber nicht von unserer App
            ///    // gestartet werden.
            ///}
            catch (Exception ex)
            {
                Android.Util.Log.Warn(
                    "HyperOS",
                    "Xiaomi-Intent fehlgeschlagen: " + ex);
            }
        }


        /*
         * -------------------------------------------------------------
         * LETZTER FALLBACK
         * -------------------------------------------------------------
         *
         * Die Android-App-Info ist NICHT die HyperOS-Autostart-Seite.
         *
         * Auf aktuellen HyperOS-Versionen befindet sich
         * "Hintergrund-Autostart" teilweise unter:
         *
         * Einstellungen
         *   -> Apps
         *      -> Berechtigungen
         *         -> Hintergrund-Autostart
         *
         * Daher ist dies nur ein Notfall-Fallback.
         */

        try
        {
            var intent =
                new Intent(
                    Settings.ActionApplicationDetailsSettings);

            intent.SetData(
                Android.Net.Uri.Parse(
                    "package:" + context.PackageName));

            intent.AddFlags(
                ActivityFlags.NewTask);

            context.StartActivity(intent);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(
                "HyperOS",
                "App-Info konnte nicht geöffnet werden: " + ex);
        }
    }

#endif
}
