using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProjectManagement.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
  private static readonly ConcurrentDictionary<string, (string UserId, string Email)> _tokenUserMap = new();

  public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

  public static void RegisterUser(string token, string userId, string email)
  {
    if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
    {
      _tokenUserMap[token] = (userId, email ?? "test@example.com");
    }
  }

  public static string GetUserId(string token)
  {
    if (_tokenUserMap.TryGetValue(token, out var userInfo))
    {
      return userInfo.UserId;
    }
    throw new KeyNotFoundException("Token not found");
  }

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    var authHeader = Context.Request.Headers.Authorization.FirstOrDefault();
    if (string.IsNullOrEmpty(authHeader))
    {
      return Task.FromResult(AuthenticateResult.Fail("No authorization header"));
    }

    var parts = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 2 || !string.Equals(parts[0], "Bearer", StringComparison.OrdinalIgnoreCase))
    {
      return Task.FromResult(AuthenticateResult.Fail("Invalid authorization header format"));
    }

    var token = parts[1];

    if (!_tokenUserMap.TryGetValue(token, out var userInfo))
    {
      return Task.FromResult(AuthenticateResult.Fail("Unknown test token"));
    }

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, userInfo.UserId),
      new Claim(ClaimTypes.Name, userInfo.Email),
      new Claim(ClaimTypes.Email, userInfo.Email),
      new Claim(ClaimTypes.Role, "User")
    };

    var identity = new ClaimsIdentity(claims, "Test");
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, "Test");

    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}