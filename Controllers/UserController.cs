using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Models;
using Microsoft.Extensions.Options;
using UserAuthApi.Options;
using UserAuthApi.Services;

namespace UserAuthApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ErrorMessageOptions _messages;

    public UserController(IAuthService authService, IOptions<ErrorMessageOptions> messages)
    {
        _authService = authService;
        _messages = messages.Value;
    }

    /// <summary>Get current user (first name, last name, email). Need valid token.</summary>
    [HttpGet("user")]
    [ProducesResponseType(typeof(CurrentUserResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 401)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        // get user id from token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ErrorResponse { Status = 401, Errors = new[] { _messages.InvalidToken } });

        var user = _authService.GetUserById(userId);
        if (user == null)
            return NotFound(new ErrorResponse { Status = 404, Errors = new[] { _messages.UserNotFound } });

        return Ok(new CurrentUserResponse
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        });
    }
}
