using KidJumpUp.Configuration;

namespace KidJumpUp.Services;

public class ConfigService
{
    public AppConfig Current { get; } = new();
}
