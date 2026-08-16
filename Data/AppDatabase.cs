using KidJumpUp.Models;
using SQLite;

namespace KidJumpUp.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection? _database;

    private readonly string _databasePath;

    public AppDatabase()
    {
        _databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "kidjumpup.db3");
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        _database = new SQLiteAsyncConnection(_databasePath);

        await InitializeAsync();

        return _database;
    }

    private async Task InitializeAsync()
    {
        if (_database == null)
            throw new InvalidOperationException(
                "Database connection is not initialized.");

        await _database.CreateTableAsync<Exercise>();
        await _database.CreateTableAsync<DailyPlan>();
        await _database.CreateTableAsync<DailyExercise>();
        await _database.CreateTableAsync<TrainingResult>();
    }

    public async Task<List<Exercise>> GetExercisesAsync()
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Exercise>()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<int> GetExerciseUsageCountAsync(int exerciseId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<DailyExercise>()
            .Where(x => x.ExerciseId == exerciseId)
            .CountAsync();
    }

    public async Task<int> SaveExerciseAsync(Exercise exercise)
    {
        var database = await GetDatabaseAsync();

        if (exercise.Id == 0)
            return await database.InsertAsync(exercise);

        return await database.UpdateAsync(exercise);
    }

    public async Task<int> DeleteExerciseAsync(Exercise exercise)
    {
        var database = await GetDatabaseAsync();

        return await database.DeleteAsync(exercise);
    }
}
