using System.Net;
using System.Net.Http.Json;
using UserAuthApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using UserAuthApi.Tests.WebApplicationFactory;
using Xunit;

namespace UserAuthApi.Tests.Integration;

internal class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201WithToken()
    {
        var email = $"register-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Integration",
            LastName = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth.Token));
        Assert.NotNull(auth.User);
        Assert.Equal(email, auth.User.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "A",
            LastName = "B"
        };
        await _client.PostAsJsonAsync("/api/auth/register", request);

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterRegister_Returns200WithToken()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        var password = "MyPassword123!";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "L",
            LastName = "O"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth.Token));
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        var email = $"badpass-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "CorrectPass1!",
            FirstName = "X",
            LastName = "Y"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_Returns200()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LogoutResponse>();
        Assert.NotNull(body);
        Assert.Contains("logged out", body.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_Returns200()
    {
        var email = $"current-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = "Pass123!",
            FirstName = "Current",
            LastName = "User"
        });
        registerResponse.EnsureSuccessStatusCode();
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
        var response = await _client.GetAsync("/api/user");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(user);
        Assert.Equal("Current", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.Equal(email, user.Email);
    }
}
