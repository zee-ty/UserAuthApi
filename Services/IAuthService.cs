using UserAuthApi.Models;

namespace UserAuthApi.Services;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    LoginResult Login(LoginRequest request);
    /// <summary>Gets user by id (e.g. from JWT sub claim). Returns null if not found.</summary>
    User? GetUserById(Guid userId);
}

public class RegisterResult
{
    public AuthResponse? Response { get; set; }
    public ErrorResponse? Error { get; set; }
    public bool IsConflict { get; set; }
    public bool IsBadRequest { get; set; }
}

public class LoginResult
{
    public AuthResponse? Response { get; set; }
    public ErrorResponse? Error { get; set; }
    public bool IsUnauthorized { get; set; }
}
