using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Application.DTOs.Tags;

public class CreateTagDto
{
  [Required(ErrorMessage = "Tag name is required")]
  [StringLength(50, MinimumLength = 1, ErrorMessage = "Tag name must be between 1-50 characters")]
  public required string Name { get; set; }

  [Required(ErrorMessage = "Tag color is required")]
  [StringLength(7, MinimumLength = 7, ErrorMessage = "Tag color must be exactly 7 characters (including #)")]
  public required string Color { get; set; }
}