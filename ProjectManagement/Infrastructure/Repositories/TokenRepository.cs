using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProjectManagement.Application.Interfaces.Repositories;

namespace ProjectManagement.Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
  private readonly IConfiguration _config;

  public TokenRepository(IConfiguration configuration)
  {
    _config = configuration;
  }

  public Task<(string token, DateTime expiresAtUtc)> CreateTokenAsync(IdentityUser user, IEnumerable<string> roles)
  {
    var key = _config["Jwt:Key"]!;
    var issuer = _config["Jwt:Issuer"]!;
    var audience = _config["Jwt:Audience"]!;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
      {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? user.Email ?? user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimTypes.NameIdentifier, user.Id)
      };
    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

    var expires = DateTime.UtcNow.AddHours(1);

    var token = new JwtSecurityToken(
      issuer: issuer,
      audience: audience,
      claims: claims,
      notBefore: DateTime.UtcNow,
      expires: expires,
      signingCredentials: creds);

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Task.FromResult((tokenString, expires));
  }
}