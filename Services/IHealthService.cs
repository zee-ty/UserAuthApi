using UserAuthApi.Models;

namespace UserAuthApi.Services;

public interface IHealthService
{
    HealthResponse GetHealth();
    VersionResponse GetVersion();
}
