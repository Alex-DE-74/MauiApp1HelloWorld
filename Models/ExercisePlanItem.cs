namespace KidJumpUp.Models;

public class ExercisePlanItem
{
    public Exercise Exercise { get; init; } = null!;

    public int ExerciseId => Exercise.Id;

    public string Name => Exercise.Name;

    public bool IsSelected { get; set; }

    public int? Target { get; set; }

    public string TargetText =>
        Target?.ToString() ?? string.Empty;
}
