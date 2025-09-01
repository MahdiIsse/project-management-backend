using System.Text.Json.Serialization;

namespace ProjectManagement.Application.DTOs.Auth;

public class RegisterResponseDto
{
  public required string Token { get; set; }
  public DateTime ExpiresAtUtc { get; set; }

  public bool OnboardingCompleted { get; set; }
  public string? OnboardingError { get; set; }

  [JsonPropertyName("message")]
  public string Message { get; set; } = "User registered successfully";
}
