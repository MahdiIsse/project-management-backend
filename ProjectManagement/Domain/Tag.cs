using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.Domain;

public class Tag
{
  public Guid Id { get; private set; }
  public string Name { get; private set; } = null!;
  public string Color { get; private set; } = null!;
  public string UserId { get; private set; } = null!;
  public DateTime CreatedAt { get; private set; }
  public DateTime? UpdatedAt { get; private set; }
  public ICollection<ProjectTask> Tasks { get; private set; } = new List<ProjectTask>();

  private Tag() { }

  [SetsRequiredMembers]
  public Tag(string name, string color, string userId)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Tag name cannot be empty");
    if (string.IsNullOrWhiteSpace(color))
      throw new ArgumentException("Tag color cannot be empty");
    if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required for tag creation");

    Id = Guid.NewGuid();
    Name = name;
    Color = color;
    UserId = userId;
    CreatedAt = DateTime.UtcNow;
  }

  public void Update(string name, string color)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Tag name cannot be empty");
    if (string.IsNullOrWhiteSpace(color))
      throw new ArgumentException("Tag color cannot be empty");

    Name = name;
    Color = color;
    UpdatedAt = DateTime.UtcNow;
  }
}