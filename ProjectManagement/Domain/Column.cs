using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.Domain;

public class Column
{
  public Guid Id { get; private set; }
  public string Title { get; private set; } = null!;
  public string? Color { get; private set; }
  public int Position { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? UpdatedAt { get; private set; }

  public Guid WorkspaceId { get; private set; }
  public Workspace? Workspace { get; private set; }

  private Column() { }

  [SetsRequiredMembers]
  public Column(string title, string? color, Guid workspaceId)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentException("Column title cannot be empty");

    if (workspaceId == Guid.Empty)
      throw new ArgumentException("Valid workspace ID is required");

    Id = Guid.NewGuid();
    Title = title;
    Color = color;
    WorkspaceId = workspaceId;
    CreatedAt = DateTime.UtcNow;
  }

  public void Update(string title, string? color, int position)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentException("Column title cannot be empty");

    Title = title;
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