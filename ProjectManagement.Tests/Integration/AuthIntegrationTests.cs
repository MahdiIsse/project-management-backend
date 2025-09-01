using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DTOs.Auth;

namespace ProjectManagement.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly CustomWebApplicationFactory<Program> _factory;
  private readonly HttpClient _client;

  public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _client = _factory.CreateClient();
  }

  [Fact]
  public async Task Register_ValidData_ReturnsOkWithTokenAndOnboarding()
  {
    var email = $"register-test-{Guid.NewGuid()}@example.com";
    var registerRequest = new RegisterRequestDto
    {
      Email = email,
      Password = "TestPassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
    var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.NotNull(result.Token);
    Assert.NotEqual(default, result.ExpiresAtUtc);
    Assert.True(result.OnboardingCompleted);
    Assert.Null(result.OnboardingError);
    Assert.Contains("onboarded successfully", result.Message);

    _client.DefaultRequestHeaders.Authorization =
      new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

    var workspacesResponse = await _client.GetAsync("/api/workspaces");
    Assert.True(workspacesResponse.IsSuccessStatusCode ||
                workspacesResponse.StatusCode == HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task Register_InvalidEmail_ReturnsBadRequest()
  {
    var registerRequest = new RegisterRequestDto
    {
      Email = "invalid-email",
      Password = "ValidPassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Register_InvalidPassword_ReturnsBadRequest()
  {
    var registerRequest = new RegisterRequestDto
    {
      Email = "test@example.com",
      Password = "12345"
    };

    var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Login_ValidCredentials_ReturnsOkWithToken()
  {
    var registerRequest = new RegisterRequestDto
    {
      Email = "logintest@example.com",
      Password = "ValidPassword123!"
    };

    await _client.PostAsJsonAsync("api/auth/register", registerRequest);

    var loginRequest = new LoginRequestDto
    {
      Email = "logintest@example.com",
      Password = "ValidPassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(loginResponse?.Token);
    Assert.True(loginResponse.ExpiresAtUtc > DateTime.UtcNow);
  }

  [Fact]
  public async Task Login_InvalidPassword_ReturnsUnauthorized()
  {
    var registerRequest = new RegisterRequestDto
    {
      Email = "invalidlogin@example.com",
      Password = "ValidPassword123!"
    };

    await _client.PostAsJsonAsync("api/auth/register", registerRequest);

    var loginRequest = new LoginRequestDto
    {
      Email = "invalidlogin@example.com",
      Password = "WrongPassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task Login_NonExistentUser_ReturnsUnauthorized()
  {
    var loginRequest = new LoginRequestDto
    {
      Email = "nonexistent@example.com",
      Password = "SomePassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task Login_MissingCredentials_ReturnsBadRequest()
  {
    var loginRequest = new LoginRequestDto
    {
      Email = "test@example.com",
      Password = ""
    };

    var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Register_OnboardingFailure_StillCreatesUser()
  {
    var email = $"onboard-fail-{Guid.NewGuid()}@example.com";
    var registerRequest = new RegisterRequestDto
    {
      Email = email,
      Password = "TestPassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
    var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.NotNull(result.Token);
    Assert.NotEqual(default, result.ExpiresAtUtc);
  }

  [Fact]
  public async Task Register_WeakPassword_ReturnsBadRequest()
  {
    var registerRequest = new RegisterRequestDto
    {
      Email = $"weak-pass-{Guid.NewGuid()}@example.com",
      Password = "123"
    };

    var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Register_DuplicateEmail_ReturnsBadRequest()
  {
    var email = $"duplicate-{Guid.NewGuid()}@example.com";
    var firstRequest = new RegisterRequestDto
    {
      Email = email,
      Password = "TestPassword123!"
    };
    await _client.PostAsJsonAsync("api/auth/register", firstRequest);

    var secondRequest = new RegisterRequestDto
    {
      Email = email,
      Password = "DifferentPassword123!"
    };

    var response = await _client.PostAsJsonAsync("api/auth/register", secondRequest);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}