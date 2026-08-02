using Android.Content;
using Android.App;

namespace MauiApp1HelloWorld;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        // Der Aufruf bleibt kurz und knackig
        ZeigeKritischeNotification(context);

        //SysAlert0(context);
        
        // Toast       
        Android.Widget.Toast.MakeText(
        context,
        "AlarmReceiver gestartet",
        Android.Widget.ToastLength.Long).Show();

        // Wenn der Wecker klingelt, holen wir die MainPage in den Vordergrund
        Intent i = new Intent(context, typeof(MainActivity));
        i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        context.StartActivity(i);

        // Startet sofort die Shake-Challenge, sobald die App aufwacht
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Shell.Current?.CurrentPage is MainPage mainPage)
            {
                mainPage.StartShakeChallenge();
            }
        });
    }

private void SysAlert0(Android.Content.Context context)
{
    // Ihr originaler, minimalistischer 5-Zeiler
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, "alarm_channel_id")
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh);

    var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
    manager?.Notify(12345, builder.Build());
}
private void ZeigeKritischeNotification(Context context)
{
    // Wir nutzen eine frische Kanal-ID, damit Android die alten Einstellungen komplett vergisst
    var channelId = "final_alarm_channel_v5";
    var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);

    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
    {
        var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
        {
            LockscreenVisibility = Android.App.NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        channel.SetBypassDnd(true);
        manager?.CreateNotificationChannel(channel);
    }

    // Der Broadcast-Trick für den Sperrbildschirm (blockiert die UI im Vordergrund nicht)
    var selfIntent = new Intent(context, typeof(AlarmReceiver));
    var fullScreenPendingIntent = PendingIntent.GetBroadcast(
        context, 
        99, 
        selfIntent, 
        PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

    // Exakt Ihr originaler Builder – erweitert um die Rechte für den Sperrbildschirm
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        
        // Garantiert das Aufploppen im Vordergrund (Ihr funktionierender Zustand):
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh)
        
        // Schützt den Sperrbildschirm und weckt ihn auf:
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
        .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm)
        .SetFullScreenIntent(fullScreenPendingIntent, true) 
        
        .SetAutoCancel(true);

    manager?.Notify(12345, builder.Build());
}
    
    private void ZeigeKritischeNotificationVx3(Android.Content.Context context)
{
    // Wir nutzen einen frischen Kanal, um jeglichen Cache-Fehler auszuschließen!
    var channelId = "final_alarm_channel_v3";
    var manager = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);

    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
    {
        var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
        {
            LockscreenVisibility = Android.App.NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        channel.SetBypassDnd(true);
        manager?.CreateNotificationChannel(channel);
    }

    // TRICK: Wir senden das FullScreenIntent an den Receiver selbst (GetBroadcast statt GetActivity!)
    var selfIntent = new Android.Content.Intent(context, typeof(AlarmReceiver));
    var fullScreenPendingIntent = Android.App.PendingIntent.GetBroadcast(
        context, 
        99, 
        selfIntent, 
        Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

    // Der perfekte Builder (Ihr originaler 5-Zeiler + Sperrbildschirm-Aufwecker)
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        
        // Holt das Banner im Vordergrund zurück (Ihr originaler Zustand):
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh)
        
        // Schützt den Sperrbildschirm und weckt ihn auf:
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
        .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm)
        .SetFullScreenIntent(fullScreenPendingIntent, true) // Jetzt als Broadcast -> blockiert im Vordergrund nicht mehr!
        
        .SetAutoCancel(true);

    manager?.Notify(12345, builder.Build());
}

    private void ZeigeKritischeNotificationVx2(Android.Content.Context context)
    {
    var channelId = "alarm_channel_id";
    var manager = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);

    // 1. Kanal einmalig mit hoher Wichtigkeit registrieren
    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
    {
        var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
        {
            LockscreenVisibility = Android.App.NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        channel.SetBypassDnd(true); 
        manager?.CreateNotificationChannel(channel);
    }

    // 2. Das Intent für Klick und Vollbild (Öffnet Ihre MainActivity)
    var intent = new Android.Content.Intent(context, typeof(MainActivity));
    intent.AddFlags(Android.Content.ActivityFlags.ClearTop);
    
    var pendingIntent = Android.App.PendingIntent.GetActivity(
        context, 
        99, 
        intent, 
        Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

    // 3. Der Builder (Kombination aus Ihrer Einfachheit + Android 14 Schutz)
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        
        // Diese Parameter garantieren das Aufpoppen im Vorder- und Hintergrund:
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) 
        .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll) 
        .SetVibrate(new long[] { 1000, 1000, 1000 }) // Harte Vibration erzwingen
        .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
        
        // WICHTIG: Das sorgt für das Aufwachen im Hintergrund UND das Banner im Vordergrund
        .SetContentIntent(pendingIntent) 
        .SetFullScreenIntent(pendingIntent, true) 
        
        .SetAutoCancel(true);

    manager?.Notify(12345, builder.Build());
}

private void ZeigeKritischeNotificationVx(Android.Content.Context context)
{
    var channelId = "alarm_channel_id";
    var manager = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);

    // 1. Kanal konfigurieren
    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
    {
        var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
        {
            LockscreenVisibility = Android.App.NotificationVisibility.Public
        };
        channel.EnableVibration(true);
        channel.SetBypassDnd(true); 
        manager?.CreateNotificationChannel(channel);
    }

    // 2. PRÜFUNG: Läuft die App gerade im Vordergrund?
    bool appIstImVordergrund = false;
    var activityManager = (Android.App.ActivityManager)context.GetSystemService(Android.Content.Context.ActivityService);
    var laufendeProzesse = activityManager?.RunningAppProcesses;
    
    if (laufendeProzesse != null)
    {
        foreach (var prozess in laufendeProzesse)
        {
            if (prozess.Importance == Android.App.Importance.Foreground && prozess.ProcessName == context.PackageName)
            {
                appIstImVordergrund = true;
                break;
            }
        }
    }

    // 3. Basis-Builder einstellen
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) 
        .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll) 
        .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
        .SetAutoCancel(true);

    // 4. HIER IST DAS ENTSCHEIDENDE IF:
    // FullScreenIntent NUR setzen, wenn die App NICHT im Vordergrund läuft!
    if (!appIstImVordergrund)
    {
        var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
            context, 
            99, 
            fullScreenIntent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);
            
        builder.SetFullScreenIntent(fullScreenPendingIntent, true); 
    }

    manager?.Notify(12345, builder.Build());
}
}
