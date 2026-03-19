using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserAuthApi.Data;
using UserAuthApi.Models;
using UserAuthApi.Options;
using UserAuthApi.Services;
using Moq;
using UserAuthApi.Tests.Helpers;

namespace UserAuthApi.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IJwtTokenService> _jwtMock;
    private readonly ErrorMessageOptions _messages;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _jwtMock = new Mock<IJwtTokenService>();
        _jwtMock.Setup(s => s.CreateToken(It.IsAny<User>())).Returns("fake-jwt-token");

        _messages = new ErrorMessageOptions();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndReturnsToken()
    {
        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var result = await sut.RegisterAsync(request);

        Assert.False(result.IsConflict);
        Assert.False(result.IsBadRequest);
        Assert.NotNull(result.Response);
        Assert.Equal("fake-jwt-token", result.Response.Token);
        Assert.Equal("test@example.com", result.Response.User.Email);
        Assert.Equal("Jane", result.Response.User.FirstName);
        Assert.Equal("Doe", result.Response.User.LastName);
        Assert.NotEqual(Guid.Empty, result.Response.User.Id);

        var saved = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(saved);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", saved.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsConflict()
    {
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            FirstName = "A",
            LastName = "B",
            PasswordHash = "hash"
        });
        await _context.SaveChangesAsync();

        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var result = await sut.RegisterAsync(request);

        Assert.True(result.IsConflict);
        Assert.NotNull(result.Error);
        Assert.Contains(_messages.UserAlreadyExists, result.Error.Errors);
    }

    [Fact]
    public async Task RegisterAsync_NullRequest_ReturnsBadRequest()
    {
        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));

        var result = await sut.RegisterAsync(null!);

        Assert.True(result.IsBadRequest);
        Assert.NotNull(result.Error);
        Assert.Contains(_messages.RequestBodyRequired, result.Error.Errors);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsToken()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("SecretPass1");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "login@example.com",
            FirstName = "F",
            LastName = "L",
            PasswordHash = passwordHash
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));
        var request = new LoginRequest { Email = "login@example.com", Password = "SecretPass1" };

        var result = sut.Login(request);

        Assert.False(result.IsUnauthorized);
        Assert.NotNull(result.Response);
        Assert.Equal("fake-jwt-token", result.Response.Token);
        Assert.Equal("login@example.com", result.Response.User.Email);
    }

    [Fact]
    public void Login_WrongPassword_ReturnsUnauthorized()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "F",
            LastName = "L",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));
        var request = new LoginRequest { Email = "user@example.com", Password = "WrongPassword" };

        var result = sut.Login(request);

        Assert.True(result.IsUnauthorized);
        Assert.NotNull(result.Error);
        Assert.Contains(_messages.InvalidEmailOrPassword, result.Error.Errors);
    }

    [Fact]
    public void Login_UnknownEmail_ReturnsUnauthorized()
    {
        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));
        var request = new LoginRequest { Email = "nobody@example.com", Password = "AnyPass1" };

        var result = sut.Login(request);

        Assert.True(result.IsUnauthorized);
        Assert.NotNull(result.Error);
        Assert.Contains(_messages.InvalidEmailOrPassword, result.Error.Errors);
    }

    [Fact]
    public void GetUserById_ExistingUser_ReturnsUser()
    {
        var id = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = id,
            Email = "get@example.com",
            FirstName = "First",
            LastName = "Last",
            PasswordHash = "hash"
        });
        _context.SaveChanges();

        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));

        var user = sut.GetUserById(id);

        Assert.NotNull(user);
        Assert.Equal(id, user.Id);
        Assert.Equal("get@example.com", user.Email);
    }

    [Fact]
    public void GetUserById_NonExistent_ReturnsNull()
    {
        var sut = new AuthService(_context, _jwtMock.Object, Microsoft.Extensions.Options.Options.Create(_messages));

        var user = sut.GetUserById(Guid.NewGuid());

        Assert.Null(user);
    }
}
