using Microsoft.Extensions.Configuration;
using UserAuthApi.Models;
using UserAuthApi.Services;

namespace UserAuthApi.Tests.Services;

public class HealthServiceTests
{
    [Fact]
    public void GetHealth_ReturnsHealthy()
    {
        var config = new ConfigurationBuilder().Build();
        var sut = new HealthService(config);

        var result = sut.GetHealth();

        Assert.Equal(HealthStatus.healthy, result.Status);
        Assert.True(result.Timestamp <= DateTime.UtcNow.AddSeconds(1));
        Assert.True(result.Timestamp >= DateTime.UtcNow.AddSeconds(-1));
    }

    [Fact]
    public void GetVersion_WhenEnvNotSet_ReturnsDev()
    {
        var config = new ConfigurationBuilder().Build();
        var sut = new HealthService(config);

        var result = sut.GetVersion();

        Assert.Equal("1.0.0", result.Version);
        Assert.Equal(EnvironmentKind.dev, result.Environment);
    }

    [Fact]
    public void GetVersion_WhenEnvIsProduction_ReturnsProd()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production"
            })
            .Build();
        var sut = new HealthService(config);

        var result = sut.GetVersion();

        Assert.Equal(EnvironmentKind.prod, result.Environment);
    }
}
