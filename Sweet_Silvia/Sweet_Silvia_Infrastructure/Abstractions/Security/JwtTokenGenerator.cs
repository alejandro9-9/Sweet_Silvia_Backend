using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Sweet_Silvia_Application.Abstractions.Security;

namespace Sweet_Silvia_Infrastructure.Abstractions.Security;

public sealed class JwtTokenGenerator(JwtOptions options) : IJwtTokenGenerator
{
    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpirationMinutes),
            signingCredentials: credentials);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
