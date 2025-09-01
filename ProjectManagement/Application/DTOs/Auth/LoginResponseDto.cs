namespace ProjectManagement.Application.DTOs.Auth;

public class LoginResponseDto
{
  public required string Token { get; set; }
  public DateTime ExpiresAtUtc { get; set; }
}