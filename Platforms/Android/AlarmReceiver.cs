using Android.Content;
using Android.App;

namespace MauiApp1HelloWorld;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
    protected override void OnCreate(Bundle? savedInstanceState)
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
    
    public override void OnReceive(Context context, Intent intent)
    {
        var prefs = context.GetSharedPreferences("alarm", FileCreationMode.Private);

        prefs.Edit()
        .PutLong("lastAlarm", Java.Lang.JavaSystem.CurrentTimeMillis())
        .Commit();
        
        // Der Aufruf bleibt kurz und knackig
        ZeigeKritischeNotificationVx(context);

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

private void ZeigeKritischeNotificationVx(Android.Content.Context context)
{
    // 1. PRÜFUNG: Läuft die App gerade aktiv im Vordergrund?
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

    // 2. DIE STRUKTURELLE WEICHE (Vollständige Trennung der Kanal-IDs, Manager und Builder)
    if (appIstImVordergrund)
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // WELT 1: APP IST OFFEN (Nutzt exklusiv die reine Vordergrund-ID)
        // ─────────────────────────────────────────────────────────────────────────────
        var vordergrundKanalId = "kanal_vordergrund_banner_v7";
        var managerVordergrund = (NotificationManager)context.GetSystemService(Context.NotificationService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(vordergrundKanalId, "App Hinweise", Android.App.NotificationImportance.High);
            managerVordergrund?.CreateNotificationChannel(channel);
        }

        // Purer 5-Zeiler (OHNE FullScreenIntent) -> Erzeugt das unblockierte Banner von oben
        var builderVordergrund = new AndroidX.Core.App.NotificationCompat.Builder(context, vordergrundKanalId)
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh)
            .SetAutoCancel(true);

        managerVordergrund?.Notify(12345, builderVordergrund.Build());
    }
    else
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // WELT 2: APP IST WEGGEWISCHT / HANDY GESPERRT (Nutzt den isolierten Wecker-Kanal)
        // ─────────────────────────────────────────────────────────────────────────────
        var weckerKanalId = "kanal_wecker_tot_v7";
        var managerHintergrund = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(weckerKanalId, "Kritische Alarme", Android.App.NotificationImportance.High)
            {
                LockscreenVisibility = Android.App.NotificationVisibility.Public
            };
            channel.EnableVibration(true);
            channel.SetBypassDnd(true); 
            managerHintergrund?.CreateNotificationChannel(channel);
        }

        var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
            context, 
            99, 
            fullScreenIntent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        // Max-Priorität und FullScreenIntent -> Weckt das Display im Tiefschlaf auf.
        // Da dieser Kanal niemals im Vordergrund genutzt wird, sperrt HyperOS die Rechte nicht!
        var builderHintergrund = new AndroidX.Core.App.NotificationCompat.Builder(context, weckerKanalId)
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) 
            .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll)  
            .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
            .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
            .SetFullScreenIntent(fullScreenPendingIntent, true) 
            .SetAutoCancel(true);

        managerHintergrund?.Notify(12345, builderHintergrund.Build());
    }
}

private void ZeigeKritischeNotificationVx8(Android.Content.Context context)
{
    // Synchronisiert auf die frische v7-ID
    var channelId = "final_alarm_channel_v7";

    // 1. PRÜFUNG: Läuft die App gerade aktiv im Vordergrund?
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

    // Das definierte Vibrationsmuster (Wichtig für den physischen Hardware-Aufruf)
    long[] vibrationsMuster = new long[] { 0, 500, 250, 500 };

    // 2. DIE WEICHE
    if (appIstImVordergrund)
    {
        // WELT 1: APP IST OFFEN
        var managerVordergrund = (NotificationManager)context.GetSystemService(Context.NotificationService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High);
            // Auch hier die Hardware-Flags spiegeln
            channel.EnableVibration(true);
            channel.SetVibrationPattern(vibrationsMuster);
            managerVordergrund?.CreateNotificationChannel(channel);
        }

        // Ihr funktionierender 5-Zeiler – jetzt mit harter Hardware-Vibration erweitert
        var builderVordergrund = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh)
            .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll) // Holt die System-Standards
            .SetVibrate(vibrationsMuster) // Erzwingt das physische Schütteln im Vordergrund
            .SetAutoCancel(true);

        managerVordergrund?.Notify(12345, builderVordergrund.Build());
    }
    else
    {
        // WELT 2: APP IST WEGGEWISCHT
        var managerHintergrund = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
            {
                LockscreenVisibility = Android.App.NotificationVisibility.Public
            };
            channel.EnableVibration(true);
            channel.SetVibrationPattern(vibrationsMuster);
            channel.SetBypassDnd(true); 
            managerHintergrund?.CreateNotificationChannel(channel);
        }

        var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
            context, 99, fullScreenIntent, Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        var builderHintergrund = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) 
            .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll) 
            .SetVibrate(vibrationsMuster) // Erzwingt das physische Schütteln, was den Sperrbildschirm weckt
            .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
            .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
            .SetFullScreenIntent(fullScreenPendingIntent, true) 
            .SetAutoCancel(true);

        managerHintergrund?.Notify(12345, builderHintergrund.Build());
    }
}

private void ZeigeKritischeNotificationVx7(Android.Content.Context context)
{
    // Die mit der MainActivity synchronisierte Kanal-ID
    var channelId = "final_alarm_channel_v6";

    // 1. PRÜFUNG: LÄUFT DIE APP AKTUELL IM VORDERGRUND?
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

    // 2. DIE STRUKTURELLE WEICHE (Vollständige Isolation von Buildern und Managern)
    if (appIstImVordergrund)
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // WELT 1: APP IST OFFEN (Vordergrund-Banner erzwingen)
        // ─────────────────────────────────────────────────────────────────────────────
        
        // Nutzt den standardmäßigen Cast, der für die AndroidX-Banner-Kompatibilität im Vordergrund sorgt
        var managerVordergrund = (NotificationManager)context.GetSystemService(Context.NotificationService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High);
            managerVordergrund?.CreateNotificationChannel(channel);
        }

        // ISOLIERTER BUILDER: Basiert exakt auf Ihrem funktionierenden, puren 5-Zeiler.
        // Enthält KEIN FullScreenIntent, da dieses unter HyperOS das Vordergrund-Banner blockiert.
        var builderVordergrund = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh) // Ihr verlässlicher Standardwert
            .SetAutoCancel(true);

        managerVordergrund?.Notify(12345, builderVordergrund.Build());
    }
    else
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // WELT 2: APP IST WEGGEWISCHT / HANDY GESPERRT (Sperrbildschirm aufwecken)
        // ─────────────────────────────────────────────────────────────────────────────
        
        // ZWINGEND: Der harte, native Android-SDK-Cast, um die komplett tote App im Kernel-Kontext zu erreichen
        var managerHintergrund = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);
        
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
            {
                LockscreenVisibility = Android.App.NotificationVisibility.Public
            };
            channel.EnableVibration(true);
            channel.SetBypassDnd(true); // Umgeht optional "Bitte nicht stören" im Tiefschlaf
            managerHintergrund?.CreateNotificationChannel(channel);
        }

        // Das Activity-Intent, welches das System zwingt, die App-Struktur im toten Zustand hochzufahren
        var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
            context, 
            99, 
            fullScreenIntent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);

        // ISOLIERTER BUILDER: Exklusiv für den Sperrbildschirm.
        // Das FullScreenIntent ist hier sicher eingesperrt und kann das Vordergrund-Banner im RAM niemals mutieren.
        var builderHintergrund = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
            .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
            .SetContentTitle("DEBUG")
            .SetContentText("AlarmReceiver wurde gestartet")
            .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) // Max-Priorität für den Kernel-Weckruf
            .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll)  // Erzwingt physische Hardware-Signale (wichtig für HyperOS)
            .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
            .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
            .SetFullScreenIntent(fullScreenPendingIntent, true) // Weckt das schwarze Display physisch auf
            .SetAutoCancel(true);

        managerHintergrund?.Notify(12345, builderHintergrund.Build());
    }
}

private void ZeigeKritischeNotificationVx6(Android.Content.Context context)
{
    var channelId = "final_alarm_channel_v6";

    // 1. PRÜFUNG: Läuft die App gerade aktiv im Vordergrund?
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

    // 2. Der Basis-Builder (Ihr funktionierender Ursprung)
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityMax) 
        .SetDefaults(AndroidX.Core.App.NotificationCompat.DefaultAll) 
        .SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm) 
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic) 
        .SetAutoCancel(true);

    // 3. DIE ABSOLUTE TRENNUNG DER MANAGER UND FLAGS
    if (appIstImVordergrund)
    {
        // FALL A: APP IST OFFEN -> AndroidX-Kompatibilitäts-Cast für das Vordergrund-Banner
        var managerVordergrund = (NotificationManager)context.GetSystemService(Context.NotificationService);
        
        // Kanal-Check im Vordergrund
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High);
            managerVordergrund?.CreateNotificationChannel(channel);
        }

        managerVordergrund?.Notify(12345, builder.Build());
    }
    else
    {
        // FALL B: APP IST WEGGEWISCHT -> Harter, nativer System-Cast, um die tote App zu retten
        var managerHintergrund = (Android.App.NotificationManager)context.GetSystemService(Android.Content.Context.NotificationService);
        
        // Kanal-Check im Hintergrund
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        {
            var channel = new Android.App.NotificationChannel(channelId, "Kritische Alarme", Android.App.NotificationImportance.High)
            {
                LockscreenVisibility = Android.App.NotificationVisibility.Public
            };
            channel.EnableVibration(true);
            channel.SetBypassDnd(true);
            managerHintergrund?.CreateNotificationChannel(channel);
        }

        // FullScreenIntent NUR im Hintergrund/Sperrbildschirm anhängen
        var fullScreenIntent = new Android.Content.Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = Android.App.PendingIntent.GetActivity(
            context, 
            99, 
            fullScreenIntent, 
            Android.App.PendingIntentFlags.UpdateCurrent | Android.App.PendingIntentFlags.Immutable);
            
        builder.SetFullScreenIntent(fullScreenPendingIntent, true); 

        managerHintergrund?.Notify(12345, builder.Build());
    }
}

private void ZeigeKritischeNotificationVx5(Context context)
{
    // Wir erhöhen auf v6, um den Cache für diesen finalen Test komplett zu leeren
    var channelId = "final_alarm_channel_v6";
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

    // 1. PRÜFUNG: Läuft die App JETZT GERADE im Vordergrund?
    bool appIstImVordergrund = false;
    var activityManager = (Android.App.ActivityManager)context.GetSystemService(Context.ActivityService);
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

    // 2. Ihr originaler, perfekt funktionierender 5-Zeiler als Basis
    var builder = new AndroidX.Core.App.NotificationCompat.Builder(context, channelId)
        .SetSmallIcon(Android.Resource.Drawable.IcLockIdleAlarm)
        .SetContentTitle("DEBUG")
        .SetContentText("AlarmReceiver wurde gestartet")
        .SetPriority(AndroidX.Core.App.NotificationCompat.PriorityHigh) // Garantiert Ihr Banner-Plopp im Vordergrund
        .SetVisibility(AndroidX.Core.App.NotificationCompat.VisibilityPublic)
        .SetAutoCancel(true);

    // 3. DIE RECHTLICHE WEICHE:
    if (!appIstImVordergrund)
    {
        // NUR wenn die App zu oder im Hintergrund ist, nutzen wir die Activity,
        // die stark genug ist, den toten Prozess und den Sperrbildschirm aufzuwecken!
        var fullScreenIntent = new Intent(context, typeof(MainActivity));
        var fullScreenPendingIntent = PendingIntent.GetActivity(
            context, 
            99, 
            fullScreenIntent, 
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            
        builder.SetFullScreenIntent(fullScreenPendingIntent, true); 
        builder.SetCategory(AndroidX.Core.App.NotificationCompat.CategoryAlarm);
    }

    manager?.Notify(12345, builder.Build());
}

private void ZeigeKritischeNotificationVx4(Context context)
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

private void ZeigeKritischeNotificationVx0(Android.Content.Context context)
{
    var channelId = "final_alarm_channel_v6";
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
