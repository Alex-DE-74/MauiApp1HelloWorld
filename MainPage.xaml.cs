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
    public async Task SetzeWeckerV2(int sekundenBisAlarm)
    {
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
            var intent = new Android.Content.Intent(context, typeof(AlarmReceiver));
            intent.AddFlags(Android.Content.ActivityFlags.IncludeStoppedPackages);

            var pendingIntent = Android.App.PendingIntent.GetBroadcast(
                context, 
                0, 
                intent, 
                Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable); 

            long triggerAtMs = Java.Lang.JavaSystem.CurrentTimeMillis() + (sekundenBisAlarm * 1000);

            if (alarmManager != null)
            {
                var alarmClockInfo = new Android.App.AlarmManager.AlarmClockInfo(triggerAtMs, pendingIntent);
                alarmManager.SetAlarmClock(alarmClockInfo, pendingIntent);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Wecker Fehler: {ex.Message}");
        }
#endif
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
