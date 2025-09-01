using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Application.DTOs.Auth;

public class RegisterRequestDto
{
  [Required(ErrorMessage = "Email is required")]
  [EmailAddress(ErrorMessage = "Please enter a valid email address")]
  public required string Email { get; set; }

  [Required(ErrorMessage = "Password is required")]
  [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
  public required string Password { get; set; }
}