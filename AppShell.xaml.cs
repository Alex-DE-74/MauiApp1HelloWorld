using KidJumpUp.Pages;
using System.Windows.Input;

namespace KidJumpUp;

public partial class AppShell : Shell
{

    public ICommand AddExerciseCommand { get; }

    public AppShell()
    {
        InitializeComponent();
        AddExerciseCommand = new Command(async () =>
        {
          if (Current?.CurrentPage is ExercisesPage page)
          {
            await page.AddExerciseAsync();
          }
        });

        BindingContext = this;

        Routing.RegisterRoute(nameof(ExercisesPage), typeof(ExercisesPage));
        Routing.RegisterRoute(nameof(ExercisePlanPage), typeof(ExercisePlanPage));
    }
}
