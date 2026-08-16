using SQLite;

namespace KidJumpUp.Models;

[Table("DailyPlan")]
public class DailyPlan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Date { get; set; } = string.Empty;
}
