using KidJumpUp.Data;
using KidJumpUp.Models;

namespace KidJumpUp.Services;

public class ExcercisePlanService
{
    private readonly AppDatabase _database;

    public ExcercisePlanService(AppDatabase database)
    {
        _database = database;
    }


    // =========================================================
    // Plan laden
    // =========================================================

    public async Task<DailyPlan?> GetPlanAsync(
        DateOnly date)
    {
        return await _database.GetDailyPlanAsync(
            ToDatabaseDate(date));
    }


    // =========================================================
    // Übungen eines Tages laden
    // =========================================================

    public async Task<List<DailyExercise>>
        GetDailyExercisesAsync(DateOnly date)
    {
        var plan = await GetPlanAsync(date);

        if (plan == null)
            return [];

        return await _database.GetDailyExercisesAsync(
            plan.Id);
    }


    // =========================================================
    // Plan speichern
    // =========================================================

    public async Task SavePlanAsync(
        DateOnly date,
        IEnumerable<ExercisePlanItem> items)
    {
        var selectedItems = items
            .Where(x => x.IsSelected)
            .ToList();

        var databaseDate = ToDatabaseDate(date);

        var plan =
            await _database.GetDailyPlanAsync(
                databaseDate);

        // -----------------------------------------------------
        // Es wurde nichts ausgewählt.
        // -----------------------------------------------------

        if (selectedItems.Count == 0)
        {
            if (plan == null)
                return;

            var existingExercises =
                await _database.GetDailyExercisesAsync(
                    plan.Id);

            foreach (var dailyExercise in existingExercises)
            {
                var resultCount =
                    await _database.GetTrainingResultCountAsync(
                        dailyExercise.Id);

                // Mit Ergebnis niemals löschen.
                if (resultCount == 0)
                {
                    await _database.DeleteDailyExerciseAsync(
                        dailyExercise);
                }
            }

            var remaining =
                await _database.GetDailyExercisesAsync(
                    plan.Id);

            if (remaining.Count == 0)
            {
                await _database.DeleteDailyPlanAsync(plan);
            }

            return;
        }


        // -----------------------------------------------------
        // DailyPlan anlegen, falls noch nicht vorhanden.
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
        // Bestehende DailyExercises laden.
        // -----------------------------------------------------

        var existing =
            await _database.GetDailyExercisesAsync(
                plan.Id);

        var selectedIds = selectedItems
            .Select(x => x.ExerciseId)
            .ToHashSet();


        // -----------------------------------------------------
        // Nicht mehr ausgewählte Übungen entfernen.
        // -----------------------------------------------------

        foreach (var dailyExercise in existing)
        {
            if (selectedIds.Contains(
                    dailyExercise.ExerciseId))
            {
                continue;
            }

            var resultCount =
                await _database.GetTrainingResultCountAsync(
                    dailyExercise.Id);

            // Historie schützen.
            if (resultCount == 0)
            {
                await _database.DeleteDailyExerciseAsync(
                    dailyExercise);
            }
        }


        // -----------------------------------------------------
        // Aktuellen Stand erneut laden.
        // -----------------------------------------------------

        existing =
            await _database.GetDailyExercisesAsync(
                plan.Id);

        var existingByExerciseId =
            existing.ToDictionary(
                x => x.ExerciseId);


        // -----------------------------------------------------
        // Neue Übungen hinzufügen bzw.
        // Target aktualisieren.
        // -----------------------------------------------------

        foreach (var item in selectedItems)
        {
            if (existingByExerciseId.TryGetValue(
                    item.ExerciseId,
                    out var dailyExercise))
            {
                // Target darf geändert werden.
                if (dailyExercise.Target != item.Target)
                {
                    dailyExercise.Target = item.Target;

                    await _database.InsertOrUpdateDailyExerciseAsync(
                        dailyExercise);
                }
            }
            else
            {
                await _database.InsertDailyExerciseAsync(
                    new DailyExercise
                    {
                        DailyPlanId = plan.Id,
                        ExerciseId = item.ExerciseId,
                        Target = item.Target
                    });
            }
        }
    }


    // =========================================================
    // Hat eine DailyExercise bereits Ergebnisse?
    // =========================================================

    public async Task<bool> HasTrainingResultsAsync(
        int dailyExerciseId)
    {
        var count =
            await _database.GetTrainingResultCountAsync(
                dailyExerciseId);

        return count > 0;
    }


    private static string ToDatabaseDate(
        DateOnly date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}
