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

}
