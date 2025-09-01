using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Tags;

namespace ProjectManagement.Application.DTOs.ProjectTasks;

public class ProjectTaskDto
{
  public Guid Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public DateTime? DueDate { get; set; }
  public required TaskPriority Priority { get; set; }
  public required int Position { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public Guid ColumnId { get; set; }
  public Guid WorkspaceId { get; set; }

  public List<AssigneeDto> Assignees { get; set; } = new();
  public List<TagDto> Tags { get; set; } = new();
}