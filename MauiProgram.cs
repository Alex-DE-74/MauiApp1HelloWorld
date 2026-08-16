using KidJumpUp.Data;
using KidJumpUp.Services;

namespace KidJumpUp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.Services.AddSingleton<AppDatabase>()
        .Services.AddSingleton<ExerciseService>();l
        .Services.AddTransient<ExercisesPage>()
		.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		return builder.Build();
	}
}
