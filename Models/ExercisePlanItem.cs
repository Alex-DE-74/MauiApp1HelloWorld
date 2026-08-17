namespace KidJumpUp.Models;

public class ExercisePlanItem
{
    public Exercise Exercise { get; set; } = null!;

    public bool IsSelected { get; set; }

    public string TargetText { get; set; } = string.Empty;
}
