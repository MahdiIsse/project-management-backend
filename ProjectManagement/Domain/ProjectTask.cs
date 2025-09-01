using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.Domain;

public enum TaskPriority
{
  Low = 1,
  Medium = 2,
  High = 3
}

public class ProjectTask
{
  public Guid Id { get; private set; }
  public string Title { get; private set; } = null!;
  public string? Description { get; private set; }
  public DateTime? DueDate { get; private set; }
  public TaskPriority Priority { get; private set; }
  public int Position { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime? UpdatedAt { get; private set; }

  public Guid ColumnId { get; private set; }
  public Column? Column { get; private set; }

  public Guid WorkspaceId { get; private set; }
  public Workspace? Workspace { get; private set; }

  public ICollection<Assignee> Assignees { get; private set; } = new List<Assignee>();
  public ICollection<Tag> Tags { get; private set; } = new List<Tag>();

  private ProjectTask() { }

  [SetsRequiredMembers]
  public ProjectTask(Guid workspaceId, Guid columnId, string title, TaskPriority priority, int position, string? description, DateTime? dueDate)
  {
    if (workspaceId == Guid.Empty)
      throw new ArgumentException("Valid workspace ID is required");

    if (columnId == Guid.Empty)
      throw new ArgumentException("Valid column ID is required");

    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentException("Task title cannot be empty");

    if (!Enum.IsDefined(typeof(TaskPriority), priority))
      throw new ArgumentException("Invalid priority value");

    Id = Guid.NewGuid();
    WorkspaceId = workspaceId;
    ColumnId = columnId;
    Title = title;
    Priority = priority;
    Description = description;
    DueDate = dueDate;
    Position = position;
    CreatedAt = DateTime.UtcNow;
  }

  public void UpdateDetails(string title, string? description, DateTime? dueDate, TaskPriority priority)
  {
    if (!Enum.IsDefined(typeof(TaskPriority), priority))
      throw new ArgumentException("Invalid priority value");

    Title = title;
    Description = description;
    DueDate = dueDate;
    Priority = priority;
    UpdatedAt = DateTime.UtcNow;
  }

  public void UpdatePosition(int newPosition)
  {
    Position = newPosition;
    UpdatedAt = DateTime.UtcNow;
  }

  public void MoveToColumn(Guid newColumnId, int newPosition)
  {
    if (newColumnId == Guid.Empty)
      throw new ArgumentException("Valid column ID is required for task movement");

    ColumnId = newColumnId;
    Position = newPosition;
    UpdatedAt = DateTime.UtcNow;
  }
}
