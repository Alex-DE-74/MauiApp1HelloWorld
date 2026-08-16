using KidJumpUp.Pages;

namespace KidJumpUp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ExercisesPage), typeof(ExercisesPage));
    }
}
