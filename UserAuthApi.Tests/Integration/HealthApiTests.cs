using System.Net;
using System.Net.Http.Json;
using UserAuthApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using UserAuthApi.Tests.WebApplicationFactory;
using Xunit;

namespace UserAuthApi.Tests.Integration;

internal class HealthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Returns200AndHealthy()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(health);
        Assert.Equal(HealthStatus.healthy, health.Status);
    }

    [Fact]
    public async Task Version_Returns200WithVersion()
    {
        var response = await _client.GetAsync("/api/health/version");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var version = await response.Content.ReadFromJsonAsync<VersionResponse>();
        Assert.NotNull(version);
        Assert.Equal("1.0.0", version.Version);
    }
}
