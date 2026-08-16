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
            .AddTransient<ExercisesPage>()
            .AddSingleton<ExcercisePlanService>()
            .AddTransient<ExercisePlanPage>();
        
        return builder.Build();
    }
}
