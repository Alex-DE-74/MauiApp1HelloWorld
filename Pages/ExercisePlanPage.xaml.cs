using KidJumpUp.Models;
using KidJumpUp.Services;

namespace KidJumpUp.Pages;

public partial class ExercisePlanPage : ContentPage
{
    private readonly ExerciseService _exerciseService;
    private readonly ExercisePlanService _exercisePlanService;

    private DateOnly _selectedDate;

    private bool _loading;

    // Noch nicht gespeicherter UI-Zustand:
    // ExerciseId -> Target
    // bald raus wenn _v x Versionen weg sind.
    private readonly Dictionary<int, int?> _selectedExercises = new();


    public ExercisePlanPage(
        ExerciseService exerciseService,
        ExercisePlanService exercisePlanService)
    {
        InitializeComponent();

        _exerciseService = exerciseService;
        _exercisePlanService = exercisePlanService;

        // Standardmäßig Morgen.
        _selectedDate = DateOnly.FromDateTime(
            DateTime.Today.AddDays(1));
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDayAsync();
    }

private async Task LoadDayAsync()
{
    if (_loading)
        return;

    _loading = true;

    try
    {
        UpdateDateDisplay();

        var planItems =
            await _exercisePlanService
                .GetPlanItemsAsync(_selectedDate);

        ExercisesCollection.ItemsSource = planItems;
    }
    finally
    {
        _loading = false;
    }
}
    private async Task LoadDayAsync_v1()
{
    if (_loading)
        return;

    _loading = true;

    try
    {
        UpdateDateDisplay();

        _selectedExercises.Clear();

        var planItems =
            await _exercisePlanService
                .GetPlanItemsAsync(_selectedDate);

        foreach (var item in planItems)
        {
            if (item.IsSelected)
            {
                _selectedExercises[item.ExerciseId] =
                    int.TryParse(
                        item.TargetText,
                        out var target)
                        ? target
                        : null;
            }
        }

        ExercisesCollection.ItemsSource = planItems;
    }
    finally
    {
        _loading = false;
    }
}

    private async Task LoadDayAsync_vO()
    {
        if (_loading)
            return;

        _loading = true;

        try
        {
            UpdateDateDisplay();

            _selectedExercises.Clear();

            var exercises =
                await _exerciseService.GetExercisesAsync();

            var dailyExercises =
                await _exercisePlanService
                    .GetDailyExercisesAsync(
                        _selectedDate);

            foreach (var dailyExercise in dailyExercises)
            {
                _selectedExercises[
                    dailyExercise.ExerciseId] =
                    dailyExercise.Target;
            }

            ExercisesCollection.ItemsSource = exercises
                .Where(x => x.IsActive)
                .ToList();

            // Die Controls werden beim Erzeugen der
            // CollectionView-Zeilen gesetzt.
        }
        finally
        {
            _loading = false;
        }
    }


    private void UpdateDateDisplay()
    {
        var today =
            DateOnly.FromDateTime(DateTime.Today);

        if (_selectedDate == today)
        {
            DayTitleLabel.Text = "Heute";
        }
        else if (_selectedDate ==
                 today.AddDays(1))
        {
            DayTitleLabel.Text = "Morgen";
        }
        else if (_selectedDate ==
                 today.AddDays(-1))
        {
            DayTitleLabel.Text = "Gestern";
        }
        else
        {
            DayTitleLabel.Text =
                _selectedDate.ToString("dd.MM.yyyy");
        }

        DateLabel.Text =
            _selectedDate.ToString("dd.MM.yyyy");
    }


    private async void OnPreviousDayClicked(
        object sender,
        EventArgs e)
    {
        _selectedDate =
            _selectedDate.AddDays(-1);

        await LoadDayAsync();
    }


    private async void OnNextDayClicked(
        object sender,
        EventArgs e)
    {
        _selectedDate =
            _selectedDate.AddDays(1);

        await LoadDayAsync();
    }

    private async void OnExerciseCheckedChanged_v1(
    object sender,
    CheckedChangedEventArgs e)
{
    if (sender is not CheckBox checkBox)
        return;

    if (checkBox.BindingContext
        is not ExercisePlanItem item)
    {
        return;
    }

    if (e.Value)
    {
        if (!_selectedExercises.ContainsKey(
                item.ExerciseId))
        {
            _selectedExercises[item.ExerciseId] = null;
        }

        if (checkBox.Parent is Grid grid)
        {
            var entry = grid.Children
                .OfType<Entry>()
                .FirstOrDefault();

            if (entry != null)
            {
                await Task.Yield();
                entry.Focus();
            }
        }
    }
    else
    {
        _selectedExercises.Remove(
            item.ExerciseId);

        if (checkBox.Parent is Grid grid)
        {
            var entry = grid.Children
                .OfType<Entry>()
                .FirstOrDefault();

            entry?.Unfocus();
        }

        item.Target = null;
    }
}

    private void OnExerciseCheckedChanged_v0(
    object sender,
    CheckedChangedEventArgs e)
{
    if (sender is not CheckBox checkBox)
        return;

    if (checkBox.BindingContext
        is not ExercisePlanItem item)
    {
        return;
    }

    if (e.Value)
    {
        if (!_selectedExercises.ContainsKey(
                item.ExerciseId))
        {
            _selectedExercises[item.ExerciseId] = null;
        }
    }
    else
    {
        _selectedExercises.Remove(
            item.ExerciseId);
    }
}

    private void OnTargetTextChanged_v1(
    object sender,
    TextChangedEventArgs e)
{
    if (sender is not Entry entry)
        return;

    if (entry.BindingContext
        is not ExercisePlanItem item)
    {
        return;
    }

    if (!_selectedExercises.ContainsKey(
            item.ExerciseId))
    {
        return;
    }

    if (int.TryParse(
            e.NewTextValue,
            out var target))
    {
        _selectedExercises[item.ExerciseId] =
            Math.Max(0, target);
    }
    else if (string.IsNullOrWhiteSpace(
                 e.NewTextValue))
    {
        _selectedExercises[item.ExerciseId] = null;
    }
}

private async void OnSaveClicked(
    object sender,
    EventArgs e)
{
    var selectedExercises =
        new Dictionary<int, int?>();

    if (ExercisesCollection.ItemsSource
        is IEnumerable<ExercisePlanItem> items)
    {
        foreach (var item in items)
        {
            if (!item.IsSelected)
                continue;

            selectedExercises[item.ExerciseId] =
                item.Target;
        }
    }

    await _exercisePlanService.SavePlanAsync(
        _selectedDate,
        selectedExercises);

    await DisplayAlert(
        "Gespeichert",
        $"Der Plan für {GetDayDescription()} wurde gespeichert.",
        "OK");

    await LoadDayAsync();
}

    private async void OnSaveClicked_v1(
        object sender,
        EventArgs e)
    {
        await _exercisePlanService.SavePlanAsync(
            _selectedDate,
            new Dictionary<int, int?>(
                _selectedExercises));

        await DisplayAlert(
            "Gespeichert",
            $"Der Plan für {GetDayDescription()} wurde gespeichert.",
            "OK");

        await LoadDayAsync();
    }


    private string GetDayDescription()
    {
        var today =
            DateOnly.FromDateTime(DateTime.Today);

        if (_selectedDate == today)
            return "heute";

        if (_selectedDate ==
            today.AddDays(1))
        {
            return "morgen";
        }

        if (_selectedDate ==
            today.AddDays(-1))
        {
            return "gestern";
        }

        return _selectedDate.ToString(
            "dd.MM.yyyy");
    }
}
