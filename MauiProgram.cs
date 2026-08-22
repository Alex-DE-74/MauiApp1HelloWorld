using Microsoft.Maui.Platform;

using KidJumpUp.Data;
using KidJumpUp.Services;
using KidJumpUp.Pages;

namespace KidJumpUp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddSingleton<AppDatabase>()
            .AddSingleton<ExerciseService>()
            .AddSingleton<ConfigService>()
            .AddTransient<ExercisesPage>()
            .AddSingleton<ExercisePlanService>()
            .AddTransient<ExercisePlanPage>();
#if ANDROID
Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("FixDerivedEntries", (handler, view) =>
{
    if (view is Microsoft.Maui.Controls.Entry && view.GetType().Namespace?.StartsWith("KidJumpUp") == true)
    {
        if (handler.PlatformView is Android.Widget.EditText nativeEdit && view is Entry entry)
        {
            // 1. Textfarbe (das hat vorher stabil funktioniert)
            if (entry.TextColor != null && entry.TextColor != Colors.Transparent)
            {
                nativeEdit.SetTextColor(new Android.Graphics.Color(
                    (byte)(entry.TextColor.Red * 255),
                    (byte)(entry.TextColor.Green * 255),
                    (byte)(entry.TextColor.Blue * 255),
                    (byte)(entry.TextColor.Alpha * 255)
                ));
            }

            // 2. Platzhalter-Farbe absolut absturzsicher mit Null-Prüfung
            if (entry.PlaceholderColor != null && entry.PlaceholderColor != Colors.Transparent)
            {
                nativeEdit.SetHintTextColor(new Android.Graphics.Color(
                    (byte)(entry.PlaceholderColor.Red * 255),
                    (byte)(entry.PlaceholderColor.Green * 255),
                    (byte)(entry.PlaceholderColor.Blue * 255),
                    (byte)(entry.PlaceholderColor.Alpha * 255)
                ));
            }
        }
    }
});
#endif

        
        return builder.Build();
    }
}
