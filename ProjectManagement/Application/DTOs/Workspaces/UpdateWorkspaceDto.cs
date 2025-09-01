using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Application.DTOs.Workspaces;
public class UpdateWorkspaceDto
{
  [Required]
  [MaxLength(100)]
  public required string Title { get; set; }

  [MaxLength(500)]
  public string? Description { get; set; }

  [StringLength(7, MinimumLength = 7, ErrorMessage = "Color must be exactly 7 characters (including #)")]
  public string? Color { get; set; }
  public int Position { get; set; }
}