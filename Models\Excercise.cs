using SQLite;

namespace KidJumpUp.Models;

[Table("Exercise")]
public class Exercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
