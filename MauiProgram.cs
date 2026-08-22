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
    // Greift für alle deine Custom Entries, die von Entry abgeleitet sind
    if (view is Microsoft.Maui.Controls.Entry && view.GetType().Namespace?.StartsWith("KidJumpUp") == true)
    {
        if (handler.PlatformView is Android.Widget.EditText nativeEdit)
        {
            // Erzwingt, dass die Textfarbe von Android niemals als Hint/Platzhalter-Farbe interpretiert wird
            if (view is Entry entry && entry.TextColor != Colors.Transparent)
            {
                nativeEdit.SetTextColor(entry.TextColor.ToPlatform());
            }
        }
    }
    });
    #endif

        
        return builder.Build();
    }
}
