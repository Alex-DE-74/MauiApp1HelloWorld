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

		//StartShakeChallenge();
		SetzeWecker(10);
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
}
