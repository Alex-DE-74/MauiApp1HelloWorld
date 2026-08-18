using KidJumpUp.Models;

namespace KidJumpUp.Pages;

public partial class ExercisePlanItemView : ContentView
{
    public ExercisePlanItemView()
    {
        InitializeComponent();
    }

    private void OnCheckedChanged(
        object sender,
        CheckedChangedEventArgs e)
    {
        if (BindingContext is not ExercisePlanItem item)
            return;

        if (e.Value)
        {
            // Übung ausgewählt:
            // Ziel ist zunächst leer.
            item.TargetText = string.Empty;

            // Entry ist durch das Binding jetzt aktiviert.
            TargetEntry.Focus();
        }
        else
        {
            // Übung abgewählt:
            // Ziel verwerfen.
            item.TargetText = string.Empty;
        }
    }

    private void OnTargetTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (BindingContext is not ExercisePlanItem item)
            return;

        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            item.Target = null;
            return;
        }

        if (int.TryParse(
                e.NewTextValue,
                out var target))
        {
            item.Target = target;
        }
        else
        {
            item.Target = null;
        }
    }
}
