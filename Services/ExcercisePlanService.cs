using KidJumpUp.Data;
using KidJumpUp.Models;

namespace KidJumpUp.Services;

public class ExercisePlanService
{
    private readonly AppDatabase _database;

    public ExercisePlanService(AppDatabase database)
    {
        _database = database;
    }


    public async Task<DailyPlan?> GetPlanAsync(DateOnly date)
    {
        return await _database.GetDailyPlanAsync(
            ToDatabaseDate(date));
    }


    public async Task<List<DailyExercise>> GetDailyExercisesAsync(
        DateOnly date)
    {
        var plan = await GetPlanAsync(date);

        if (plan == null)
            return [];

        return await _database.GetDailyExercisesAsync(
            plan.Id);
    }


    public async Task SavePlanAsync(
        DateOnly date,
        Dictionary<int, int> selectedExercises)
    {
        var databaseDate = ToDatabaseDate(date);

        var plan = await _database.GetDailyPlanAsync(
            databaseDate);


        // -----------------------------------------------------
        // Keine Übung ausgewählt
        // -----------------------------------------------------

        if (selectedExercises.Count == 0)
        {
            if (plan == null)
                return;

            var existingExercises =
                await _database.GetDailyExercisesAsync(plan.Id);

            foreach (var dailyExercise in existingExercises)
            {
                var resultCount =
                    await _database.GetTrainingResultCountAsync(
                        dailyExercise.Id);

                // Bereits vorhandene Ergebnisse schützen.
                if (resultCount == 0)
                {
                    await _database.DeleteDailyExerciseAsync(
                        dailyExercise);
                }
            }

            var remaining =
                await _database.GetDailyExercisesAsync(plan.Id);

            if (remaining.Count == 0)
            {
                await _database.DeleteDailyPlanAsync(plan);
            }

            return;
        }


        // -----------------------------------------------------
        // DailyPlan bei Bedarf anlegen
        // -----------------------------------------------------

        if (plan == null)
        {
            plan = new DailyPlan
            {
                Date = databaseDate
            };

            await _database.InsertDailyPlanAsync(plan);
        }


        // -----------------------------------------------------
        // Bestehende DailyExercises
        // -----------------------------------------------------

        var existing =
            await _database.GetDailyExercisesAsync(plan.Id);

        var existingByExerciseId =
            existing.ToDictionary(x => x.ExerciseId);


        // -----------------------------------------------------
        // Nicht mehr ausgewählte Übungen entfernen
        // -----------------------------------------------------

        foreach (var dailyExercise in existing)
        {
            if (selectedExercises.ContainsKey(
                    dailyExercise.ExerciseId))
            {
                continue;
            }

            var resultCount =
                await _database.GetTrainingResultCountAsync(
                    dailyExercise.Id);

            // Niemals eine DailyExercise mit Ergebnis löschen.
            if (resultCount == 0)
            {
                await _database.DeleteDailyExerciseAsync(
                    dailyExercise);
            }
        }


        // -----------------------------------------------------
        // Neue Übungen hinzufügen /
        // Ziele bestehender Übungen aktualisieren
        // -----------------------------------------------------

        foreach (var selected in selectedExercises)
        {
            var exerciseId = selected.Key;
            var target = selected.Value;

            if (existingByExerciseId.TryGetValue(
                    exerciseId,
                    out var dailyExercise))
            {
                if (dailyExercise.Target != target)
                {
                    dailyExercise.Target = target;

                    await _database.SaveDailyExerciseAsync(
                        dailyExercise);
                }
            }
            else
            {
                await _database.InsertDailyExerciseAsync(
                    new DailyExercise
                    {
                        DailyPlanId = plan.Id,
                        ExerciseId = exerciseId,
                        Target = target
                    });
            }
        }
    }


    public async Task<bool> HasTrainingResultsAsync(
        int dailyExerciseId)
    {
        var count =
            await _database.GetTrainingResultCountAsync(
                dailyExerciseId);

        return count > 0;
    }


    private static string ToDatabaseDate(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}
