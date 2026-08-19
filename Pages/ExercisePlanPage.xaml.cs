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
    //private readonly Dictionary<int, int?> _selectedExercises = new();


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
