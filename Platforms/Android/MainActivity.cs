using Android.App;
using Android.Content.PM;
using Android.OS;

namespace MauiApp1HelloWorld;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        ShowAlarmTime();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                "kanal_vordergrund_banner_v7",
                "MauiApp1HelloWorld",
                NotificationImportance.High);

            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }
    }

    protected void ShowAlarmTime()
    {
        base.OnCreate(savedInstanceState);

        var prefs = GetSharedPreferences("alarm", FileCreationMode.Private);

        long lastAlarm = prefs.GetLong("lastAlarm", 0);

        string text;

        if (lastAlarm == 0)
        {
            text = "Receiver wurde bisher NICHT ausgeführt.";
        }
        else
        {
            var zeit = DateTimeOffset
                .FromUnixTimeMilliseconds(lastAlarm)
                .LocalDateTime;

            text = $"Receiver zuletzt: {zeit:dd.MM.yyyy HH:mm:ss}";
        }

        Toast.MakeText(this, text, ToastLength.Long)?.Show();

        new Handler(Looper.MainLooper).PostDelayed(() =>
        {
            Toast.MakeText(this, text, ToastLength.Long)?.Show();
        }, 3500);

        new Handler(Looper.MainLooper).PostDelayed(() =>
        {
            Toast.MakeText(this, text, ToastLength.Long)?.Show();
        }, 7000);
    }

}
