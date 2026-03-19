using Microsoft.Extensions.Options;
using UserAuthApi.Models;
using UserAuthApi.Options;
using UserAuthApi.Services;
using System.IdentityModel.Tokens.Jwt;

namespace UserAuthApi.Tests.Services;

public class JwtTokenServiceTests
{
    private const string TestSecret = "ThisIsALongSecretKeyForTestingPurposesOnly!!";

    [Fact]
    public void CreateToken_ReturnsValidJwtWithUserClaims()
    {
        var options = new JwtOptions
        {
            Secret = TestSecret,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };
        var sut = new JwtTokenService(Microsoft.Extensions.Options.Options.Create(options));
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "jwt@example.com",
            FirstName = "J",
            LastName = "W",
            PasswordHash = "ignored"
        };

        var token = sut.CreateToken(user);

        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal("jwt@example.com", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(options.Issuer, jwt.Issuer);
        Assert.Equal(options.Audience, jwt.Audiences.First());
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }
}
