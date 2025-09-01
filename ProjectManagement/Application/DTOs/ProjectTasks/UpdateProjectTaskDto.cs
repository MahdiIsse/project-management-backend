using System.ComponentModel.DataAnnotations;
using ProjectManagement.Domain;

namespace ProjectManagement.Application.DTOs.ProjectTasks;

public class UpdateProjectTaskDto
{
  [Required]
  [MaxLength(200)]
  public required string Title { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  public DateTime? DueDate { get; set; }

  [Required]
  [EnumDataType(typeof(TaskPriority))]
  public required TaskPriority Priority { get; set; }

  [Required]
  [Range(1, int.MaxValue)]
  public required int Position { get; set; }
}