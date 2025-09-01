namespace ProjectManagement.Domain;

public class Workspace
{
  public Guid Id { get; private set; }
  public string Title { get; private set; } = null!;
  public string? Description { get; private set; }
  public string? Color { get; private set; }
  public int Position { get; private set; }
  public string UserId { get; private set; } = null!;
  public DateTime CreatedAt { get; private set; }
  public DateTime? UpdatedAt { get; private set; }

  private Workspace() { }

  public Workspace(string title, string? description, string? color, string userId)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentException("Workspace title cannot be empty");

    if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required for workspace creation");

    Id = Guid.NewGuid();
    Title = title;
    Description = description;
    Color = color;
    UserId = userId;
    CreatedAt = DateTime.UtcNow;
  }

  public void Update(string title, string? description, string? color, int position)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentException("Workspace title cannot be empty");

    Title = title;
    Description = description;
    Color = color;
    Position = position;
    UpdatedAt = DateTime.UtcNow;
  }

  public void SetPosition(int position)
  {
    Position = position;
    UpdatedAt = DateTime.UtcNow;
  }
}