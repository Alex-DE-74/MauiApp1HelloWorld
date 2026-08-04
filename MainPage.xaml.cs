/*
using System;
using System.Threading.Tasks;
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
//using System.Threading.Tasks;

namespace MauiApp1HelloWorld;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

    // KORREKTUR: override wieder aktiv. Startet, sobald die App geladen ist
    // Startet vollautomatisch, sobald das Menü auf dem Bildschirm erscheint
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Sensor-Rechte direkt beim allerersten Start abfragen
        //var status = await Permissions.CheckStatusAsync<Permissions.Sensors>();
        //if (status != PermissionStatus.Granted)
        //{
        //    await Permissions.RequestAsync<Permissions.Sensors>();
        //}

        // 2. PARALLELE BEGRÜSSUNG: Über den Dispatcher abgesichert, damit die UI nicht einfriert
        Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(500); // Gibt der UI kurz Zeit zum Rendern

            _ = Task.Run(async () =>
            {
                try
                {
                    await TextToSpeech.Default.SpeakAsync("Hallo Elina! Schön dass du da bist. Lass uns den Tag fit verbringen");
                }
                catch { } // Lautlos fangen, falls Hardware blockiert 
            });

            // ERFÜLLT DIE COMPILER-WARNUNG: Nutzt das von .NET 10 verlangte DisplayAlertAsync
            await this.DisplayAlertAsync("Hallo Elina! 🏃‍♂️", "Schön dass du da bist. Lass uns den Tag fit verbringen", "Los geht's! 🚀");
        });
    }
	
	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
    private void OnChallengeClicked(object sender, EventArgs e)
	{
		count+=10;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);

		StartShakeChallenge();
		//SetzeWeckerV2(10);
	}
    private int _shakeCount = 0;
    private bool _isAlarmActive = false;

    public void StartShakeChallenge()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _isAlarmActive = true;
            _shakeCount = 0;
            CounterBtn.Text = "Schütteln: 0 / 10";

            if (Accelerometer.Default.IsSupported && !Accelerometer.Default.IsMonitoring)
            {
                Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
        });
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        var data = e.Reading;
        double gForce = Math.Sqrt(data.Acceleration.X * data.Acceleration.X + 
                                  data.Acceleration.Y * data.Acceleration.Y + 
                                  data.Acceleration.Z * data.Acceleration.Z);

        if (gForce > 1.5) 
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!_isAlarmActive) return;

                _shakeCount++;
                CounterBtn.Text = $"Schütteln: {_shakeCount} / 10";

                if (_shakeCount >= 10)
                {
                    StopShakeChallenge();
                    CounterBtn.Text = "🎉 Geschafft! 🎉";
                    _isAlarmActive = false;
                    _shakeCount = 0;
                }
            });
        }
    }

    private void StopShakeChallenge()
    {
        if (Accelerometer.Default.IsSupported && Accelerometer.Default.IsMonitoring)
        {
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.Stop();
        }
    }

    // Rufen Sie diese Methode in Ihrer "OnAlarmStellen"-Button-Methode auf
    public void SetzeWecker(int sekundenBisAlarm)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var intent = new Android.Content.Intent(context, typeof(AlarmReceiver));
        var pendingIntent = Android.App.PendingIntent.GetBroadcast(
            context, 
            0, 
            intent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        var alarmManager = (Android.App.AlarmManager)context.GetSystemService(Android.Content.Context.AlarmService);
        
        long triggerAtMs = Java.Lang.JavaSystem.CurrentTimeMillis() + (sekundenBisAlarm * 1000);

        if (alarmManager != null)
        {
            // KORREKTUR: Nutzt die unter .NET 10 korrekte Android-Konstante direkt aus dem AlarmManager
            alarmManager.SetExactAndAllowWhileIdle(Android.App.AlarmType.RtcWakeup, triggerAtMs, pendingIntent);
        }
#endif
    }

#if ANDROID

public async Task PruefeUndOeffneAutostartWennNoetigAsync()
{
    var context = Android.App.Application.Context;

    // SIKHERHEITS-CHECK 1: Ist es überhaupt ein Xiaomi / HyperOS Gerät?
    // (Prüft den Hersteller-Eintrag des Geräts)
    string hersteller = Build.Manufacturer?.ToLower() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

    if (!istXiaomi)
    {
        // Wenn es kein Xiaomi ist, brechen wir hier sofort ab. Keine Endlosschleife möglich!
        return; 
    }

    // SICHERHEITS-CHECK 2: Ist der Android-Standard-Schalter überhaupt noch aktiv?
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            // Status abfragen (0 = Aktiv/Eingeschränkt, 1 oder 2 = Sicher/Bereits deaktiviert)
            int status = await IntentCompat.GetUnusedAppRestrictionsStatusAsync(context);
            
            // WENN Status == 0, dann ist der Schalter AKTIV und blockiert uns.
            // NUR DANN leiten wir den Nutzer weiter!
            if (status == 0)
            {
                // UI-Hinweis im Haupt-Thread von MAUI anzeigen
                await Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(async () =>
                {
                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "HyperOS Optimierung",
                        "Bitte deaktiviere im nächsten Bildschirm die Option 'App-Aktivität bei Nichtbenutzung pausieren', damit deine Wecker nach dem Wegwischen zuverlässig funktionieren.",
                        "Zu den Einstellungen");
                });

                // Jetzt rufen wir deine Methode auf, da wir wissen, dass es nötig ist
                ResolveHyperOsAutostartRestriction();
            }
            else
            {
                // Status ist 1 oder 2 -> Der Nutzer hat es schon deaktiviert!
                // Wir tun nichts und verhindern die Endlosschleife.
                System.Diagnostics.Debug.WriteLine("HyperOS-Schalter ist bereits sicher deaktiviert.");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler bei der Abfrage: {ex.Message}");
        }
    }
}

// Deine angepasste Methode (wird nur aufgerufen, wenn oben alles zutrifft)
private void ResolveHyperOsAutostartRestriction()
{
    var context = Android.App.Application.Context;
    var intent = new Intent();
    
    intent.SetComponent(new ComponentName(
        "com.miui.securitycenter", 
        "com.miui.permcenter.autostart.AutoStartManagementActivity"));
        
    intent.AddFlags(ActivityFlags.NewTask);

    try
    {
        context.StartActivity(intent);
    }
    catch (System.Exception)
    {
        try 
        {
            var fallbackIntent = new Intent("miui.intent.action.OP_AUTO_START");
            fallbackIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(fallbackIntent);
        }
        catch 
#if ANDROID

public async Task PruefeUndOeffneAutostartWennNoetigAsync()
{
    var context = Android.App.Application.Context;

    // SICHERHEITS-CHECK 1: Ist es überhaupt ein Xiaomi / HyperOS Gerät?
    string hersteller = Build.Manufacturer?.ToLower() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

    if (!istXiaomi) return; 

    // SICHERHEITS-CHECK 2: Ist der Android-Standard-Schalter aktiv?
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            // FEHLERBEHEBUNG: Die Abfrage liegt in 'PackageManagerCompat' statt 'IntentCompat'
            var future = PackageManagerCompat.GetUnusedAppRestrictionsStatus(context);
            
            // Konvertiert das Java-Future in einen für C# await-baren Task
            int status = (int)await Task.Run(() => future.Get());
            
            // 0 steht für UnusedAppRestrictionsConstants.StatusRestricted (Schalter ist an)
            if (status == 0) 
            {
                await Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(async () =>
                {
                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "HyperOS Optimierung",
                        "Bitte deaktiviere im nächsten Bildschirm die Option 'App-Aktivität bei Nichtbenutzung pausieren', damit deine Wecker nach dem Wegwischen zuverlässig funktionieren.",
                        "Zu den Einstellungen");
                });

                ResolveHyperOsAutostartRestriction();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler bei der Hibernation-Abfrage: {ex.Message}");
        }
    }
}

private void ResolveHyperOsAutostartRestriction()
{
    var context = Android.App.Application.Context;
    
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            // IntentCompat ist korrekt für das Erzeugen des Einstellungs-Intents zuständig
            var intent = IntentCompat.CreateManageUnusedAppRestrictionsIntent(context, context.PackageName);
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (System.Exception)
        {
            // Letzter Fallback auf Xiaomi-Sicherheitszentrum direkt
            TryOpenXiaomiSecurityDirectly(context);
        }
    }
}

private void TryOpenXiaomiSecurityDirectly(Context context)
{
    var intent = new Intent();
#if ANDROID
using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using System.Threading.Tasks;

public async Task PruefeUndOeffneAutostartWennNoetigAsync()
{
    var context = Android.App.Application.Context;

    // SICHERHEITS-CHECK 1: Ist es überhaupt ein Xiaomi / HyperOS Gerät?
    string hersteller = Build.Manufacturer?.ToLower() ?? "";
    bool istXiaomi = hersteller.Contains("xiaomi") || hersteller.Contains("redmi") || hersteller.Contains("poco");

    if (!istXiaomi) return; 

    // SICHERHEITS-CHECK 2: Ist der Android-Standard-Schalter aktiv?
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            // FEHLERBEHEBUNG: Die Abfrage liegt in 'PackageManagerCompat' statt 'IntentCompat'
            var future = PackageManagerCompat.GetUnusedAppRestrictionsStatus(context);
            
            // Konvertiert das Java-Future in einen für C# await-baren Task
            int status = (int)await Task.Run(() => future.Get());
            
            // 0 steht für UnusedAppRestrictionsConstants.StatusRestricted (Schalter ist an)
            if (status == 0) 
            {
                await Microsoft.Maui.Controls.Application.Current.Dispatcher.DispatchAsync(async () =>
                {
                    await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                        "HyperOS Optimierung",
                        "Bitte deaktiviere im nächsten Bildschirm die Option 'App-Aktivität bei Nichtbenutzung pausieren', damit deine Wecker nach dem Wegwischen zuverlässig funktionieren.",
                        "Zu den Einstellungen");
                });

                ResolveHyperOsAutostartRestriction();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler bei der Hibernation-Abfrage: {ex.Message}");
        }
    }
}

private void ResolveHyperOsAutostartRestriction()
{
    var context = Android.App.Application.Context;
    
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
        try
        {
            // IntentCompat ist korrekt für das Erzeugen des Einstellungs-Intents zuständig
            var intent = IntentCompat.CreateManageUnusedAppRestrictionsIntent(context, context.PackageName);
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }
        catch (System.Exception)
        {
            // Letzter Fallback auf Xiaomi-Sicherheitszentrum direkt
            TryOpenXiaomiSecurityDirectly(context);
        }
    }
}

private void TryOpenXiaomiSecurityDirectly(Context context)
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
            // Absoluter Notanker: Normale App-Info-Seite
            var appInfoIntent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
            var uri = Android.Net.Uri.FromParts("package", context.PackageName, null);
            appInfoIntent.SetData(uri);
            appInfoIntent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(appInfoIntent);
        }
    }
}
#endif


    public async Task SetzeWeckerV2(int sekundenBisAlarm)
    {
	/*
	var contextBo = Android.App.Application.Context;
	var intentBo = new Android.Content.Intent(
        Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations,
        Android.Net.Uri.Parse("package:" + contextBo.PackageName));

    intentBo.AddFlags(Android.Content.ActivityFlags.NewTask);
	
	contextBo.StartActivity(intentBo);
	*/

	await PruefeUndOeffneAutostartWennNoetigAsync();
#if ANDROID
    // Android 13+: Benachrichtigungsberechtigung anfordern
    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
    {
        var status = await Permissions.RequestAsync<Permissions.PostNotifications>();

        if (status != PermissionStatus.Granted)
        {
            await this.DisplayAlertAsync(
                "Benachrichtigungen erforderlich",
                "Damit der Wecker Statusmeldungen anzeigen kann, müssen Benachrichtigungen erlaubt werden.",
                "OK");

            return;
        }
    }
#endif	
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var alarmManager = (Android.App.AlarmManager)context.GetSystemService(Android.Content.Context.AlarmService);

            // 1. WECKER-PRÜFUNG (Prüft exakte Alarme ab Android 12)
            if (alarmManager != null && Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                if (!alarmManager.CanScheduleExactAlarms())
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        bool oeffnen = await this.DisplayAlertAsync(
                            "Berechtigung nötig ⚠️", 
                            "Bitte erlaube der App in den Android-Einstellungen, exakte Wecker zu stellen.", 
                            "Zu den Einstellungen", 
                            "Abbrechen");

                        if (oeffnen)
                        {
                            var intentSettings = new Android.Content.Intent(Android.Provider.Settings.ActionRequestScheduleExactAlarm);
                            intentSettings.AddFlags(Android.Content.ActivityFlags.NewTask);
                            context.StartActivity(intentSettings);
                        }
                    });
                    return;
                }
            }

            // 2. SPERRBILDSCHIRM-PRÜFUNG (Overlay-Berechtigung)
            if (!Android.Provider.Settings.CanDrawOverlays(context))
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool oeffnenSperrbildschirm = await this.DisplayAlertAsync(
                        "Sperrbildschirm-Recht nötig 🔓", 
                        "Damit der Wecker bei ausgeschaltetem Bildschirm anspringt, muss die App 'über anderen Apps eingeblendet' werden dürfen.", 
                        "Zu den Einstellungen", 
                        "Abbrechen");

                    if (oeffnenSperrbildschirm)
                    {
                        var intentOverlay = new Android.Content.Intent(
                            Android.Provider.Settings.ActionManageOverlayPermission,
                            Android.Net.Uri.Parse($"package:{context.PackageName}"));
                        intentOverlay.AddFlags(Android.Content.ActivityFlags.NewTask);
                        context.StartActivity(intentOverlay);
                    }
                });
                return; 
            }

            // 3. WECKER RECHTSKONFORM STELLEN (Exakt 4 Parameter in .NET MAUI)
            // Ersetzt Ihren alten Code komplett durch diesen Einzeiler:
            StarteExaktenWecker(context, sekundenBisAlarm);

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Wecker Fehler: {ex.Message}");
        }
#endif
    }

     public void StarteExaktenWecker(Android.Content.Context context, long sekundenBisAlarm)
    {
        var alarmManager = (Android.App.AlarmManager)context.GetSystemService(Android.Content.Context.AlarmService);
    
        if (alarmManager == null) return;

        // 1. Der Empfänger für den eigentlichen Alarm (BroadcastReceiver)
        var intent = new Android.Content.Intent(context, typeof(AlarmReceiver));
        // intent.AddFlags(Android.Content.ActivityFlags.IncludeStoppedPackages);
        int requestCode = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
        var pendingIntent = Android.App.PendingIntent.GetBroadcast(
            context,
            requestCode, 
            intent, 
            Android.App.PendingIntentFlags.Immutable); 

        // 2. NEU: Ziel bei Klick auf das System-Weckersymbol (Öffnet die MainActivity)
        var mainActivityIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var showIntent = Android.App.PendingIntent.GetActivity(
            context, 
            1, // Eigener RequestCode zur eindeutigen Trennung
            mainActivityIntent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        // Zeitberechnung
        long triggerAtMs = Java.Lang.JavaSystem.CurrentTimeMillis() + (sekundenBisAlarm * 1000);

        // Wecker rechtskonform stellen (Mit getrennten Intents)
        var alarmClockInfo = new Android.App.AlarmManager.AlarmClockInfo(triggerAtMs, showIntent);
        alarmManager.SetAlarmClock(alarmClockInfo, pendingIntent);
    }
	
   // NEUE METHODE: Berechnet die Zeit bis zur Ziel-Uhrzeit und startet den Wecker
    public async Task SetzeWeckerUhrzeit(int stunde, int minute)
    {
        DateTime jetzt = DateTime.Now;
        
        // Erstellt das Ziel-Datum für heute mit der gewünschten Uhrzeit
        DateTime zielZeit = new DateTime(jetzt.Year, jetzt.Month, jetzt.Day, stunde, minute, 0);

        // Falls die Uhrzeit für heute schon vorbei ist, stellen wir den Wecker für morgen ein
        if (zielZeit <= jetzt)
        {
            zielZeit = zielZeit.AddDays(1);
        }

        // Berechnet die Differenz in Sekunden
        TimeSpan zeitDifferenz = zielZeit - jetzt;
        int sekundenBisAlarm = (int)zeitDifferenz.TotalSeconds;

        // Ruft Ihre bestehende, funktionierende Methode mit den berechneten Sekunden auf
        await SetzeWeckerV2(sekundenBisAlarm);
    }
private async void OnAlarmStellenClicked(object sender, EventArgs e)
{
    // Holt die vom Benutzer im Menü ausgewählte Stunde und Minute
    int ausgewaehlteStunde = MeinTimePicker.Time.Value.Hours;
    int ausgewaehlteMinute = MeinTimePicker.Time.Value.Minutes;

    // Ruft Ihre Berechnungsmethode mit den dynamischen Werten auf
    await SetzeWeckerUhrzeit(ausgewaehlteStunde, ausgewaehlteMinute);
}
	
}
