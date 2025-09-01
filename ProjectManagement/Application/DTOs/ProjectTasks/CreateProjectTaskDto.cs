using System.ComponentModel.DataAnnotations;
using ProjectManagement.Domain;

namespace ProjectManagement.Application.DTOs.ProjectTasks;

public class CreateProjectTaskDto
{
  [Required(ErrorMessage = "Title is required")]
  [MaxLength(200)]
  public required string Title { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }

  public DateTime? DueDate { get; set; }

  [Required(ErrorMessage = "Priority is required")]
  [EnumDataType(typeof(TaskPriority), ErrorMessage = "Priority must be Low, Medium, or High")]
  public required TaskPriority Priority { get; set; }
}