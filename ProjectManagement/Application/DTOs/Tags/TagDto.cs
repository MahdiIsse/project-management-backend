namespace ProjectManagement.Application.DTOs.Tags;

public class TagDto
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public string? Color { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }

}