using System.Security.Claims;

namespace Sweet_Silvia_Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(IEnumerable<Claim> claims);
}
