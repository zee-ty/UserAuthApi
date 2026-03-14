using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Models;
using Microsoft.Extensions.Options;
using UserAuthApi.Options;
using UserAuthApi.Services;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ErrorMessageOptions _messages;

    public AuthController(IAuthService authService, IOptions<ErrorMessageOptions> messages)
    {
        _authService = authService;
        _messages = messages.Value;
    }

    /// <summary>Register new user</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 409)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<AuthResponse>> RegisterUser([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ValidationErrorResponse(ModelState));
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (result.IsConflict)
            return Conflict(result.Error);
        if (result.IsBadRequest)
            return BadRequest(result.Error);
        return StatusCode(201, result.Response);
    }

    /// <summary>User login.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public ActionResult<AuthResponse> LoginUser([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ValidationErrorResponse(ModelState));
        var result = _authService.Login(request);
        if (result.IsUnauthorized)
            return Unauthorized(result.Error);
        return Ok(result.Response);
    }

    /// <summary>Logout - client just drops the token</summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponse), 200)]
    public ActionResult<LogoutResponse> LogoutUser()
    {
        return Ok(new LogoutResponse { Message = "Successfully logged out" });
    }

    // build error list from modelstate
    private ErrorResponse ValidationErrorResponse(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrEmpty(m))
            .ToList();
        if (errors.Count == 0)
            errors.Add(_messages.ValidationFailed);
        return new ErrorResponse { Status = 400, Errors = errors };
    }
}
