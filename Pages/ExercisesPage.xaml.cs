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

    // bisheriger Inhalt von OnAddExerciseClicked(
    private void AddExercise()
    {
        _editingExercise = null;

        EditorTitle.Text = "Neue Übung";
        ExerciseNameEntry.Text = string.Empty;

        ExerciseEditorOverlay.IsVisible = true;

        ExerciseNameEntry.Focus();
    }

    private void OnAddExerciseClicked(object sender, EventArgs e)
    {
        AddExercise();
    }
    
    // Für Shell Item
    public async Task AddExerciseAsync()
    {
        AddExercise();
        return Task.CompletedTask;
    }

    private void OnEditExerciseClicked(object sender, EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not Exercise exercise)
            return;

        _editingExercise = exercise;

        EditorTitle.Text = "Übung bearbeiten";
        ExerciseNameEntry.Text = exercise.Name;

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

    private async void OnActiveChanged(
        object sender,
        CheckedChangedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        if (checkBox.BindingContext is not Exercise exercise)
            return;

        exercise.IsActive = e.Value;

        await _exerciseService.SaveExerciseAsync(exercise);
    }

    private async void OnDeleteExerciseClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        if (button.BindingContext is not Exercise exercise)
            return;

        // Noch einmal aktuell aus der Datenbank prüfen.
        var usageCount =
            await _exerciseService.GetUsageCountAsync(exercise.Id);

        if (usageCount > 0)
        {
            await DisplayAlert(
                "⚠ Trainingsplan vorhanden",
                "Diese Übung wurde bereits in einem Trainingsplan " +
                "verwendet und kann deshalb nicht gelöscht werden.",
                "OK");

            return;
        }

        var confirmed = await DisplayAlert(
            "Übung löschen?",
            $"„{exercise.Name}“ wirklich löschen?",
            "Löschen",
            "Abbrechen");

        if (!confirmed)
            return;

        var deleted =
            await _exerciseService.DeleteExerciseAsync(exercise);

        if (!deleted)
        {
            // Zwischen Prüfung und Löschung könnte theoretisch
            // ein anderer Vorgang die Übung verwendet haben.
            await DisplayAlert(
                "⚠ Löschen nicht möglich",
                "Die Übung wurde inzwischen in einem Trainingsplan " +
                "verwendet und kann deshalb nicht gelöscht werden.",
                "OK");

            await LoadExercisesAsync();

            return;
        }

        await LoadExercisesAsync();
    }

    private void OnExerciseNameCompleted(
        object sender,
        EventArgs e)
    {
        ExerciseNameEntry.Unfocus();
    }

    private void CloseEditor()
    {
        ExerciseNameEntry.Unfocus();

#if ANDROID
        var handler = ExerciseNameEntry.Handler;

        if (handler?.PlatformView is Android.Widget.EditText editText)
        {
            var inputMethodManager =
                Android.App.Application.Context
                    .GetSystemService(
                        Android.Content.Context.InputMethodService)
                as Android.Views.InputMethods.InputMethodManager;

            inputMethodManager?.HideSoftInputFromWindow(
                editText.WindowToken,
                Android.Views.InputMethods.HideSoftInputFlags.None);
        }
#endif

        ExerciseEditorOverlay.IsVisible = false;

        ExerciseNameEntry.Text = string.Empty;

        _editingExercise = null;
    }
}
