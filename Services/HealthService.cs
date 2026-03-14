using UserAuthApi.Models;

namespace UserAuthApi.Services;

public class HealthService : IHealthService
{
    private readonly IConfiguration _configuration;

    public HealthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public HealthResponse GetHealth()
    {
        return new HealthResponse
        {
            Status = HealthStatus.healthy,
            Timestamp = DateTime.UtcNow
        };
    }

    public VersionResponse GetVersion()
    {
        var env = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "dev";
        return new VersionResponse
        {
            Version = "1.0.0",
            Environment = Enum.TryParse<EnvironmentKind>(env, true, out var e) ? e : EnvironmentKind.dev
        };
    }
}
