#nullable enable
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

    // =========================================================
// DailyPlan
// =========================================================

public async Task<DailyPlan?> GetDailyPlanAsync(string date)
{
    var database = await GetDatabaseAsync();

    return await database
        .Table<DailyPlan>()
        .Where(x => x.Date == date)
        .FirstOrDefaultAsync();
}

public async Task<int> InsertDailyPlanAsync(
    DailyPlan dailyPlan)
{
    var database = await GetDatabaseAsync();

    return await database.InsertAsync(dailyPlan);
}

public async Task<int> DeleteDailyPlanAsync(
    DailyPlan dailyPlan)
{
    var database = await GetDatabaseAsync();

    return await database.DeleteAsync(dailyPlan);
}


// =========================================================
// DailyExercise
// =========================================================

public async Task<List<DailyExercise>>
    GetDailyExercisesAsync(int dailyPlanId)
{
    var database = await GetDatabaseAsync();

    return await database
        .Table<DailyExercise>()
        .Where(x => x.DailyPlanId == dailyPlanId)
        .ToListAsync();
}

public async Task<int> InsertDailyExerciseAsync(
    DailyExercise dailyExercise)
{
    var database = await GetDatabaseAsync();

    return await database.InsertAsync(dailyExercise);
}

public async Task<int> DeleteDailyExerciseAsync(
    DailyExercise dailyExercise)
{
    var database = await GetDatabaseAsync();

    return await database.DeleteAsync(dailyExercise);
}

public async Task<int> SaveDailyExerciseAsync(
    DailyExercise dailyExercise)
{
    var database = await GetDatabaseAsync();

    if (dailyExercise.Id == 0)
        return await database.InsertAsync(dailyExercise);

    return await database.UpdateAsync(dailyExercise);
}

    
// =========================================================
// TrainingResult
// =========================================================

public async Task<int> GetTrainingResultCountAsync(
    int dailyExerciseId)
{
    var database = await GetDatabaseAsync();

    return await database
        .Table<TrainingResult>()
        .Where(x => x.DailyExerciseId == dailyExerciseId)
        .CountAsync();
}
}
