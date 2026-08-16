using KidJumpUp.Services;

namespace KidJumpUp.Pages;

public partial class ExercisePlanPage : ContentPage
{
    private readonly ExerciseService _exerciseService;
    private readonly DailyPlanService _dailyPlanService;

    private DateOnly _selectedDate;

    private bool _loading;

    public ExercisePlanPage(
        ExerciseService exerciseService,
        DailyPlanService dailyPlanService)
    {
        InitializeComponent();

        _exerciseService = exerciseService;
        _dailyPlanService = dailyPlanService;

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

            var exercises =
                await _exerciseService.GetExercisesAsync();

            var dailyExercises =
                await _dailyPlanService
                    .GetDailyExercisesAsync(
                        _selectedDate);

            var dailyByExerciseId =
                dailyExercises.ToDictionary(
                    x => x.ExerciseId);

            var items = exercises
                .Where(x => x.IsActive)
                .Select(x =>
                {
                    dailyByExerciseId.TryGetValue(
                        x.Id,
                        out var dailyExercise);

                    return new ExercisePlanItem
                    {
                        ExerciseId = x.Id,
                        Name = x.Name,
                        IsSelected =
                            dailyExercise != null,
                        Target =
                            dailyExercise?.Target ?? 0
                    };
                })
                .ToList();

            ExercisesCollection.ItemsSource = items;
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


    private void OnExerciseCheckedChanged(
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

        item.IsSelected = e.Value;
    }


    private void OnTargetTextChanged(
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

        if (int.TryParse(
                e.NewTextValue,
                out var target))
        {
            item.Target = Math.Max(0, target);
        }
        else if (string.IsNullOrWhiteSpace(
                     e.NewTextValue))
        {
            item.Target = 0;
        }
    }


    private async void OnSaveClicked(
        object sender,
        EventArgs e)
    {
        if (ExercisesCollection.ItemsSource
            is not IEnumerable<ExercisePlanItem> items)
        {
            return;
        }

        await _dailyPlanService.SavePlanAsync(
            _selectedDate,
            items);

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

        return _selectedDate
            .ToString("dd.MM.yyyy");
    }
}


public class ExercisePlanItem
{
    public int ExerciseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    public int Target { get; set; }

    public string TargetText
        => Target == 0
            ? string.Empty
            : Target.ToString();
}
