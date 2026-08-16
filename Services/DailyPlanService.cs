using KidJumpUp.Data;
using KidJumpUp.Models;

namespace KidJumpUp.Services;

public class DailyPlanService
{
    private readonly AppDatabase _database;

    public DailyPlanService(AppDatabase database)
    {
        _database = database;
    }

    public async Task<DailyPlan?> GetPlanAsync(DateOnly date)
    {
        return await _database.GetDailyPlanAsync(
            ToDatabaseDate(date));
    }

    public async Task<List<DailyExercise>>
        GetExercisesAsync(DateOnly date)
    {
        return await _database.GetDailyExercisesAsync(
            ToDatabaseDate(date));
    }

    public async Task<bool> HasPlanAsync(DateOnly date)
    {
        var plan = await GetPlanAsync(date);

        return plan != null;
    }

    public async Task SavePlanAsync(
        DateOnly date,
        IEnumerable<int> selectedExerciseIds)
    {
        var databaseDate = ToDatabaseDate(date);

        var selectedIds = selectedExerciseIds
            .Distinct()
            .ToHashSet();

        var existingPlan =
            await _database.GetDailyPlanAsync(databaseDate);

        var existingExercises =
            await _database.GetDailyExercisesAsync(databaseDate);

        // Kein ausgewähltes Training.
        if (selectedIds.Count == 0)
        {
            if (existingPlan == null)
                return;

            foreach (var dailyExercise in existingExercises)
            {
                var resultCount =
                    await _database.GetTrainingResultCountAsync(
                        databaseDate,
                        dailyExercise.ExerciseId);

                // Mit Ergebnis niemals entfernen.
                if (resultCount == 0)
                {
                    await _database.DeleteDailyExerciseAsync(
                        dailyExercise);
                }
            }

            var remainingExercises =
                await _database.GetDailyExercisesAsync(
                    databaseDate);

            // DailyPlan nur löschen, wenn tatsächlich
            // keine Übungen mehr dazugehören.
            if (remainingExercises.Count == 0)
            {
                await _database.DeleteDailyPlanAsync(
                    existingPlan);
            }

            return;
        }

        // DailyPlan anlegen, falls noch keiner existiert.
        if (existingPlan == null)
        {
            existingPlan = new DailyPlan
            {
                Date = databaseDate
            };

            await _database.InsertDailyPlanAsync(
                existingPlan);
        }

        // Nicht mehr ausgewählte Übungen entfernen.
        foreach (var dailyExercise in existingExercises)
        {
            if (selectedIds.Contains(
                    dailyExercise.ExerciseId))
            {
                continue;
            }

            var resultCount =
                await _database.GetTrainingResultCountAsync(
                    databaseDate,
                    dailyExercise.ExerciseId);

            // Ergebnis vorhanden → nicht löschen.
            if (resultCount == 0)
            {
                await _database.DeleteDailyExerciseAsync(
                    dailyExercise);
            }
        }

        // Aktuellen Stand erneut laden.
        var currentExercises =
            await _database.GetDailyExercisesAsync(
                databaseDate);

        var existingExerciseIds = currentExercises
            .Select(x => x.ExerciseId)
            .ToHashSet();

        // Neue Übungen hinzufügen.
        foreach (var exerciseId in selectedIds)
        {
            if (existingExerciseIds.Contains(exerciseId))
                continue;

            await _database.InsertDailyExerciseAsync(
                new DailyExercise
                {
                    Date = databaseDate,
                    ExerciseId = exerciseId
                });
        }
    }

    public async Task<bool> HasTrainingResultsAsync(
        DateOnly date,
        int exerciseId)
    {
        var count =
            await _database.GetTrainingResultCountAsync(
                ToDatabaseDate(date),
                exerciseId);

        return count > 0;
    }

    private static string ToDatabaseDate(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}
