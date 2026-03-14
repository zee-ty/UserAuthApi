using UserAuthApi.Models;

namespace UserAuthApi.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
