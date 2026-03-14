using Microsoft.AspNetCore.Mvc;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api")]
public class OpenApiController : ControllerBase
{
    /// <summary>Return OpenAPI specification.</summary>
    [HttpGet("openapi")]
    [ProducesResponseType(200)]
    public IActionResult GetOpenApiSpec()
    {
        return Redirect("~/swagger/v1/swagger.json");
    }
}
