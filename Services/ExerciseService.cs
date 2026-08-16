using KidJumpUp.Data;
using KidJumpUp.Models;

namespace KidJumpUp.Services;

public class ExerciseService
{
    private readonly AppDatabase _database;

    public ExerciseService(AppDatabase database)
    {
        _database = database;
    }

    public async Task<List<Exercise>> GetExercisesAsync()
    {
        var exercises = await _database.GetExercisesAsync();

        foreach (var exercise in exercises)
        {
            exercise.UsageCount =
                await _database.GetExerciseUsageCountAsync(exercise.Id);
        }

        return exercises;
    }

    public async Task SaveExerciseAsync(Exercise exercise)
    {
        if (string.IsNullOrWhiteSpace(exercise.Name))
            throw new ArgumentException(
                "Der Übungsname darf nicht leer sein.");

        exercise.Name = exercise.Name.Trim();

        await _database.SaveExerciseAsync(exercise);
    }

    public async Task<int> GetUsageCountAsync(int exerciseId)
    {
        return await _database.GetExerciseUsageCountAsync(exerciseId);
    }

    public async Task<bool> DeleteExerciseAsync(Exercise exercise)
    {
        // Sicherheitsprüfung noch einmal im Service.
        // Die UI darf niemals die alleinige Schutzschicht sein.
        var usageCount =
            await _database.GetExerciseUsageCountAsync(exercise.Id);

        if (usageCount > 0)
            return false;

        var result =
            await _database.DeleteExerciseAsync(exercise);

        return result > 0;
    }
}
