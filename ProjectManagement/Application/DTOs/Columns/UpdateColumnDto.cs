using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Application.DTOs.Columns;

public class UpdateColumnDto
{
  [Required]
  [StringLength(100, MinimumLength = 1)]
  public required string Title { get; set; }
  [StringLength(7, ErrorMessage = "Color must be exactly 7 characters (including #)")]
  public string? Color { get; set; }

  [Range(0, int.MaxValue)]
  public int Position { get; set; }
}