using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserAuthApi.Data;
using UserAuthApi.Models;
using UserAuthApi.Options;

namespace UserAuthApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ErrorMessageOptions _messages;

    public AuthService(AppDbContext context, IJwtTokenService jwtTokenService, IOptions<ErrorMessageOptions> messages)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _messages = messages.Value;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return new RegisterResult { IsBadRequest = true, Error = new ErrorResponse { Status = 400, Errors = new[] { _messages.RequestBodyRequired } } };

        // check if email already taken
        if (await _context.Users.AnyAsync(u => EF.Functions.ILike(u.Email, request.Email), cancellationToken))
            return new RegisterResult
            {
                IsConflict = true,
                Error = new ErrorResponse { Status = 409, Errors = new[] { _messages.UserAlreadyExists } }
            };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)  // dont store plain text
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.CreateToken(user);

        return new RegisterResult
        {
            Response = new AuthResponse { Token = token, User = user }
        };
    }

    public LoginResult Login(LoginRequest request)
    {
        // always check database for user by email first
        var user = _context.Users.FirstOrDefault(u => EF.Functions.ILike(u.Email, request.Email));
        if (user == null)
            return new LoginResult
            {
                IsUnauthorized = true,
                Error = new ErrorResponse { Status = 401, Errors = new[] { _messages.InvalidEmailOrPassword } }
            };
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new LoginResult
            {
                IsUnauthorized = true,
                Error = new ErrorResponse { Status = 401, Errors = new[] { _messages.InvalidEmailOrPassword } }
            };

        var token = _jwtTokenService.CreateToken(user);

        return new LoginResult
        {
            Response = new AuthResponse { Token = token, User = user }
        };
    }

    public User? GetUserById(Guid userId)
    {
        return _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
    }
}
