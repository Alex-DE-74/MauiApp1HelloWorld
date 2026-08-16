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

    // Nur für die Darstellung in der Übungen-Seite.
    // Diese Werte werden nicht in der Datenbank gespeichert.
    [Ignore]
    public int UsageCount { get; set; }

    [Ignore]
    public bool CanDelete => UsageCount == 0;
}
