namespace ProjectManagement.Application.DTOs.Columns;

public class ColumnDto
{
  public Guid Id { get; set; }
  public required string Title { get; set; }
  public string? Color { get; set; }
  public int Position { get; set; }
  public DateTime CreatedAt { get; set; }
  public Guid WorkspaceId { get; set; }
}