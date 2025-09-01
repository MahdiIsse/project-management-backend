namespace ProjectManagement.Application.DTOs.Assignees;

public class AssigneeDto
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public string? AvatarUrl { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

}