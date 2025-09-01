using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Application.DTOs.Assignees;

public class CreateAssigneeDto
{
  [Required(ErrorMessage = "Name is required")]
  [MaxLength(20)]
  public required string Name { get; set; }
  public string? AvatarUrl { get; set; }
}