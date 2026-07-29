/*
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;

#if ANDROID
using Android.App;
using Android.Content;

// HIER ERGÄNZT: Der Compiler schreibt das nun absolut fehlerfrei ins Manifest!
//[assembly: UsesPermission(Android.Manifest.Permission.BodySensors)]
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
/*
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
*/	
	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}

