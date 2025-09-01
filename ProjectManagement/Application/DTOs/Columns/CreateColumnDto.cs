using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Application.DTOs.Columns;

public class CreateColumnDto
{
  [Required]
  [StringLength(100, MinimumLength = 1)]
  public required string Title { get; set; }

  [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be exactly 7 characters (including #)")]
  public string? Color { get; set; }
}