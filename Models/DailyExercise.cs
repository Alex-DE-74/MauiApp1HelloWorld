using SQLite;

namespace KidJumpUp.Models;

[Table("DailyExercise")]
public class DailyExercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public int DailyPlanId { get; set; }

    [NotNull]
    public int ExerciseId { get; set; }

    [NotNull]
    public int Target { get; set; }
}
