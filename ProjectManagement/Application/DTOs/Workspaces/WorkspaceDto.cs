namespace ProjectManagement.Application.DTOs.Workspaces;
public class WorkspaceDto
{
  public Guid Id { get; set; }
  public required string Title { get; set; }
  public string? Description { get; set; }
  public string? Color { get; set; }
  public int Position { get; set; }
  public DateTime CreatedAt { get; set; }
}