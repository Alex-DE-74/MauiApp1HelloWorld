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
        return await _database.GetExercisesAsync();
    }

    public async Task SaveExerciseAsync(Exercise exercise)
    {
        if (string.IsNullOrWhiteSpace(exercise.Name))
            throw new ArgumentException("Der Übungsname darf nicht leer sein.");

        exercise.Name = exercise.Name.Trim();

        await _database.SaveExerciseAsync(exercise);
    }
}
