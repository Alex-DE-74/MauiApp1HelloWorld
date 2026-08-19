using Microsoft.Maui;

namespace KidJumpUp.Configuration;

public class AppConfig
{
    public double ExerciseFontSize { get; set; } = 20;

    public double ExerciseTargetFontSize { get; set; } = 20;

    public Thickness ExerciseRowPadding { get; set; } =
        new(0, 4);

    public Color ExerciseTargetPlaceholderColor { get; set; } =
        Colors.Gray;
}
