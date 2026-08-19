using KidJumpUp.Services;

namespace KidJumpUp;

public partial class App : Application
{
    public App(ConfigService configService)
    {
        InitializeComponent();

        Resources["AppConfig"] = configService.Current;
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
