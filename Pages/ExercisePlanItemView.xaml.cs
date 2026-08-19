using KidJumpUp.Models;

namespace KidJumpUp.Pages;

public partial class ExercisePlanItemView : ContentView
{
    public ExercisePlanItemView()
    {
        InitializeComponent();
    }

private async void OnCheckedChanged(
    object sender,
    CheckedChangedEventArgs e)
{
    if (BindingContext is not ExercisePlanItem item)
        return;

    item.Target = null;

    if (e.Value)
    {
        await Task.Yield();
        TargetEntry.Focus();
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
