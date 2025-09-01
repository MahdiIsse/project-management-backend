using Microsoft.AspNetCore.Identity;

namespace ProjectManagement.Application.Interfaces.Repositories;
public interface ITokenRepository
{
  Task<(string token, DateTime expiresAtUtc)> CreateTokenAsync(IdentityUser user, IEnumerable<string> roles);
}
