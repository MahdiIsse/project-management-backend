using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.Domain;

public class Assignee
{
  public Guid Id { get; private set; }
  public string Name { get; private set; } = null!;
  public string? AvatarUrl { get; private set; }
  public string UserId { get; private set; } = null!;
  public DateTime CreatedAt { get; private set; }
  public DateTime? UpdatedAt { get; private set; }
  public ICollection<ProjectTask> Tasks { get; private set; } = new List<ProjectTask>();

  private Assignee() { }

  [SetsRequiredMembers]
  public Assignee(string name, string userId, string? avatarUrl)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Assignee name cannot be empty");

    if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required for assignee creation");

    Id = Guid.NewGuid();
    Name = name;
    AvatarUrl = avatarUrl;
    UserId = userId;
    CreatedAt = DateTime.UtcNow;
  }

  public void Update(string name, string? avatarUrl)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Assignee name cannot be empty");

    Name = name;
    AvatarUrl = avatarUrl;
    UpdatedAt = DateTime.UtcNow;
  }
}