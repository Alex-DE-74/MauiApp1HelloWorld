using KidJumpUp.Models;
using KidJumpUp.Services;

namespace KidJumpUp.Pages;

public partial class ExercisesPage : ContentPage
{
    private readonly ExerciseService _exerciseService;

    private Exercise? _editingExercise;

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

    private void OnAddExerciseClicked(object sender, EventArgs e)
    {
        _editingExercise = null;

        EditorTitle.Text = "Neue Übung";
        ExerciseNameEntry.Text = string.Empty;

        ExerciseEditorOverlay.IsVisible = true;

        ExerciseNameEntry.Focus();
    }

    private void OnCancelExerciseClicked(object sender, EventArgs e)
    {
        CloseEditor();
    }

    private async void OnSaveExerciseClicked(object sender, EventArgs e)
    {
        var name = ExerciseNameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert(
                "Übung",
                "Bitte einen Übungsnamen eingeben.",
                "OK");

            return;
        }

        if (_editingExercise == null)
        {
            var exercise = new Exercise
            {
                Name = name,
                IsActive = true
            };

            await _exerciseService.SaveExerciseAsync(exercise);
        }
        else
        {
            _editingExercise.Name = name;

            await _exerciseService.SaveExerciseAsync(_editingExercise);
        }

        CloseEditor();

        await LoadExercisesAsync();
    }

    private void CloseEditor()
    {
        ExerciseEditorOverlay.IsVisible = false;
        ExerciseNameEntry.Text = string.Empty;
        _editingExercise = null;
    }
}
