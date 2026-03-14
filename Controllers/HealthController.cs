using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Models;
using UserAuthApi.Services;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>Health check for container orchestration.</summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public ActionResult<HealthResponse> HealthCheck()
    {
        var response = _healthService.GetHealth();
        return Ok(response);
    }

    /// <summary>Get API version.</summary>
    [HttpGet("health/version")]
    [ProducesResponseType(typeof(VersionResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public ActionResult<VersionResponse> GetVersion()
    {
        var response = _healthService.GetVersion();
        return Ok(response);
    }
}
