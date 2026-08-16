using SQLite;

namespace KidJumpUp.Models;

[Table("TrainingResult")]
public class TrainingResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public int DailyExerciseId { get; set; }

    public int? Result { get; set; }
}
