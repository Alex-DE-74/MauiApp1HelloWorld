using KidJumpUp.Models;
using KidJumpUp.Services;

namespace KidJumpUp.Pages;

public partial class ExercisesPage : ContentPage
{
    private readonly ExerciseService _exerciseService;

    public ExercisesPage(ExerciseService exerciseService)
    {
        InitializeComponent();

        _exerciseService = exerciseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadExercisesAsync();
    }

    private async Task LoadExercisesAsync()
    {
        var exercises = await _exerciseService.GetExercisesAsync();

        ExercisesCollection.ItemsSource = exercises;
    }

    private async void OnAddExerciseClicked(object sender, EventArgs e)
    {
        // Kommt im nächsten Schritt:
        // Floating Editor anzeigen
    }
}
