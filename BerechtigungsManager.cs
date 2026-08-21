using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace KidJumpUp;

public class BerechtigungsManager
{
    private bool _warteAufEinstellung = false;
    private TaskCompletionSource<bool> _tcs = null!;

    public async Task<bool> PruefeAlleBerechtigungenAsync()
    {
        // 1. HyperOS Hintergrund-Autostart prüfen
        bool autostartGeoeffnet = await HyperOsOpenAi.PruefeUndOeffneEchtenHyperOsAutostartAsync();
        if (autostartGeoeffnet)
        {
            await WarteAufAppResumeAsync();
            return await PruefeAlleBerechtigungenAsync(); // REKURSION
        }

        // 2. Benachrichtigungsberechtigung prüfen
        bool notificationErfolgreich = await PruefePostNotificationsBerechtigungAsync();
        if (!notificationErfolgreich)
            return false;

#if ANDROID
        var context = Android.App.Application.Context;

        // 3. Exakte Alarme prüfen (Android 12+)
        bool alarmErfolgreich = await PruefeScheduleExactAlarmBerechtigungAsync(context);
        if (!alarmErfolgreich)
        {
            await WarteAufAppResumeAsync();
            return await PruefeAlleBerechtigungenAsync(); // REKURSION
        }

        // 4. Sperrbildschirm / Overlay prüfen
        bool overlayErfolgreich = await PruefeOverlayBerechtigungAsync(context);
        if (!overlayErfolgreich)
        {
            await WarteAufAppResumeAsync();
            return await PruefeAlleBerechtigungenAsync(); // REKURSION
        }
#endif

        return true; // Alle Berechtigungen sind aktiv!
    }

    private Task WarteAufAppResumeAsync()
    {
        _tcs = new TaskCompletionSource<bool>();
        _warteAufEinstellung = true;
        
        // KORREKTUR: Die fehlerhaften Zeilen '_warpAufEinstellung = false;' wurden restlos gelöscht
        var app = Microsoft.Maui.Controls.Application.Current;
        if (app != null && app.Windows.Count > 0)
        {
            app.Windows[0].Resumed += OnAppResumed;
        }
        else
        {
            _tcs.TrySetResult(false);
        }
        
        return _tcs.Task;
    }

    private void OnAppResumed(object sender, EventArgs e)
    {
        var app = Microsoft.Maui.Controls.Application.Current;
        if (app != null && app.Windows.Count > 0)
        {
            app.Windows[0].Resumed -= OnAppResumed;
        }

        if (_warteAufEinstellung)
        {
            _warteAufEinstellung = false;
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _tcs.TrySetResult(true);
            });
        }
    }

private async Task<bool> PruefePostNotificationsBerechtigungAsync()
{
#if ANDROID
    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
    {
        // 1. Status live aus dem System prüfen
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        // 2. System-Popup anfordern
        status = await Permissions.RequestAsync<Permissions.PostNotifications>();

        if (status != PermissionStatus.Granted)
        {
            bool darfNochMalFragen = Permissions.ShouldShowRationale<Permissions.PostNotifications>();
            var page = Microsoft.Maui.Controls.Application.Current?.MainPage;

            if (darfNochMalFragen)
            {
                // ─────────────────────────────────────────────────────────────────
                // PFAD 1: NUTZER HAT IM SYSTEM-POPUP DIREKT ABGELEHNT
                // ─────────────────────────────────────────────────────────────────
                if (page != null)
                {
                    // HIER DEIN HINWEIS: Erklärt dem Nutzer sofort, warum der Wecker abgebrochen wurde.
                    await page.DisplayAlertAsync(
                        "Benachrichtigungen erforderlich",
                        "Damit der Wecker Statusmeldungen anzeigen kann, müssen Benachrichtigungen erlaubt werden.",
                        "OK");
                }
                return false; // Kette stoppen
            }
            else
            {
                // ─────────────────────────────────────────────────────────────────
                // PFAD 2: SYSTEM HAT POPUP VERSCHLUCKT -> UMRULEITUNG IN APP-INFO
                // ─────────────────────────────────────────────────────────────────
                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        "Benachrichtigungen dauerhaft gesperrt",
                        "Das System blockiert das automatische Pop-up, da die Anfrage zuvor mehrfach abgelehnt wurde.\n\n"
                        + "Bitte aktiviere die Benachrichtigungen manuell im nächsten Bildschirm unter 'Benachrichtigungen'.",
                        "Zu den Einstellungen");
                }

                // Ab in die App-Info
                HyperOsOpenAi.OeffneAppInfo();
                
                // Code einfrieren, bis der Nutzer die App wieder betritt
                await WarteAufAppResumeAsync();

                // 3. Echte Live-Prüfung NACH der Rückkehr aus den Einstellungen
                var statusNachEinstellung = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

                if (statusNachEinstellung != PermissionStatus.Granted)
                {
                    // NUTZER HAT ES AUCH IN DEN APP-SETTINGS NICHT ERLAUBT
                    if (page != null)
                    {
                        // HIER DER ZWEITE HINWEIS: Erklärt nach der Rückkehr das Scheitern.
                        await page.DisplayAlertAsync(
                            "Achtung ⚠️",
                            "Die Berechtigung wurde in den Einstellungen nicht aktiviert!\n\n"
                            + "Ohne Benachrichtigungen kann der Wecker keine Statusmeldungen anzeigen. "
                            + "Bitte versuche es erneut, wenn du den Wecker stellen möchtest.",
                            "Verstanden");
                    }
                    return false; // Kette stoppen, kein endloser Loop!
                }
                else
                {
                    // Nutzer war brav -> Rekursion startet, um mit Schritt 3 (Exakte Alarme) weiterzumachen
                    return await PruefeAlleBerechtigungenAsync();
                }
            }
        }
    }
#endif
    return true;
}

    
    private async Task<bool> PruefePostNotificationsBerechtigungAsync_v0()
    {
#if ANDROID
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
        {
            var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        "Benachrichtigungen erforderlich",
                        "Damit der Wecker Statusmeldungen anzeigen kann, müssen Benachrichtigungen erlaubt werden.",
                        "OK");
                }
                return false;
            }
        }
#endif
        return true;
    }

#if ANDROID

private async Task<bool> PruefeScheduleExactAlarmBerechtigungAsync(Android.Content.Context context)
{
    var alarmManager = (Android.App.AlarmManager)context.GetSystemService(Android.Content.Context.AlarmService);

    if (alarmManager != null && Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
    {
        // ---------------------------------------------------------------------
        // LIVE-CHECK: AppOpsManager statt unzuverlässigem AlarmManager
        // ---------------------------------------------------------------------
        bool hatDasRechtWirklich = false;
        try
        {
            var appOps = (Android.App.AppOpsManager)context.GetSystemService(Android.Content.Context.AppOpsService);
            
            if (appOps != null)
            {
                // KORREKTUR: Datentyp auf AppOpsManagerMode geändert, um den impliziten Konvertierungsfehler zu beheben
                Android.App.AppOpsManagerMode mode = appOps.CheckOpNoThrow("android:schedule_exact_alarm", context.ApplicationInfo.Uid, context.PackageName);
                
                // KORREKTUR: Vergleich korrigiert. Beide Operanden nutzen jetzt den korrekten Enum-Typ
                hatDasRechtWirklich = (mode == Android.App.AppOpsManagerMode.Allowed);
            }
        }
        catch (Exception)
        {
            // Fallback, falls der AppOps-Check fehlschlägt
            hatDasRechtWirklich = alarmManager.CanScheduleExactAlarms();
        }

        // Wenn das Recht im System aktiv ist, brechen wir SOFORT erfolgreich ab.
        if (hatDasRechtWirklich)
        {
            return true;
        }

        // ---------------------------------------------------------------------
        // DER DIALOG (Kommt nur, wenn das Recht laut AppOps wirklich fehlt)
        // ---------------------------------------------------------------------
        bool oeffnen = false;
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (page != null)
            {
                oeffnen = await page.DisplayAlertAsync(
                    "Berechtigung nötig ⚠️", 
                    "Bitte erlaube der App in den Android-Einstellungen, exakte Wecker zu stellen.", 
                    "Zu den Einstellungen", 
                    "Abbrechen");
            }
        });

        if (oeffnen)
        {
            var intentSettings = new Android.Content.Intent(Android.Provider.Settings.ActionRequestScheduleExactAlarm);
            intentSettings.AddFlags(Android.Content.ActivityFlags.NewTask);
            context.StartActivity(intentSettings);
            return false;
        }
        return false;
    }
    return true;
}

    private async Task<bool> PruefeScheduleExactAlarmBerechtigungAsync_vO(Android.Content.Context context)
    {
        var alarmManager = (Android.App.AlarmManager)context.GetSystemService(Android.Content.Context.AlarmService);

        if (alarmManager != null && Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {
            if (!alarmManager.CanScheduleExactAlarms())
            {
                bool oeffnen = false;
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
                    if (page != null)
                    {
                        oeffnen = await page.DisplayAlertAsync(
                            "Berechtigung nötig ⚠️", 
                            "Bitte erlaube der App in den Android-Einstellungen, exakte Wecker zu stellen.", 
                            "Zu den Einstellungen", 
                            "Abbrechen");
                    }
                });

                if (oeffnen)
                {
                    var intentSettings = new Android.Content.Intent(Android.Provider.Settings.ActionRequestScheduleExactAlarm);
                    intentSettings.AddFlags(Android.Content.ActivityFlags.NewTask);
                    context.StartActivity(intentSettings);
                    return false;
                }
                return false;
            }
        }
        return true;
    }

    private async Task<bool> PruefeOverlayBerechtigungAsync(Android.Content.Context context)
    {
        if (!Android.Provider.Settings.CanDrawOverlays(context))
        {
            bool oeffnenSperrbildschirm = false;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var page = Microsoft.Maui.Controls.Application.Current?.MainPage;
                if (page != null)
                {
                    oeffnenSperrbildschirm = await page.DisplayAlertAsync(
                        "Sperrbildschirm-Recht nötig 🔓", 
                        "Damit der Wecker bei ausgeschaltetem Bildschirm anspringt, muss die App 'über anderen Apps eingeblendet' werden dürfen.", 
                        "Zu den Einstellungen", 
                        "Abbrechen");
                }
            });

            if (oeffnenSperrbildschirm)
            {
                var intentOverlay = new Android.Content.Intent(
                    Android.Provider.Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse($"package:{context.PackageName}"));
                intentOverlay.AddFlags(Android.Content.ActivityFlags.NewTask);
                context.StartActivity(intentOverlay);
                return false;
            }
            return false; 
        }
        return true;
    }
#endif
}
