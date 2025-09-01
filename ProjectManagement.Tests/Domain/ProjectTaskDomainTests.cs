using FluentAssertions;
using ProjectManagement.Domain;

namespace ProjectManagement.Tests.Domain;

public class ProjectTaskDomainTests
{
  [Fact]
  public void Constructor_WithValidData_CreatesSuccessfully()
  {
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var title = "Test Task";
    var priority = TaskPriority.Medium;
    var position = 1;
    var description = "Task description";
    var dueDate = DateTime.UtcNow.AddDays(7);

    var task = new ProjectTask(workspaceId, columnId, title, priority, position, description, dueDate);

    task.Should().NotBeNull();
    task.Id.Should().NotBeEmpty();
    task.WorkspaceId.Should().Be(workspaceId);
    task.ColumnId.Should().Be(columnId);
    task.Title.Should().Be(title);
    task.Priority.Should().Be(priority);
    task.Position.Should().Be(position);
    task.Description.Should().Be(description);
    task.DueDate.Should().Be(dueDate);
    task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    task.UpdatedAt.Should().BeNull();
    task.Assignees.Should().NotBeNull();
    task.Tags.Should().NotBeNull();
  }

  [Fact]
  public void Constructor_WithEmptyWorkspaceId_ThrowsArgumentException()
  {
    var emptyWorkspaceId = Guid.Empty;
    var columnId = Guid.NewGuid();

    Action act = () => new ProjectTask(emptyWorkspaceId, columnId, "Test Task", TaskPriority.Low, 1, null, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Valid workspace ID is required");
  }

  [Fact]
  public void Constructor_WithEmptyColumnId_ThrowsArgumentException()
  {
    var workspaceId = Guid.NewGuid();
    var emptyColumnId = Guid.Empty;

    Action act = () => new ProjectTask(workspaceId, emptyColumnId, "Test Task", TaskPriority.Low, 1, null, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Valid column ID is required");
  }

  [Fact]
  public void Constructor_WithEmptyTitle_ThrowsArgumentException()
  {
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();

    Action act = () => new ProjectTask(workspaceId, columnId, "", TaskPriority.Low, 1, null, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Task title cannot be empty");
  }

  [Fact]
  public void Constructor_WithInvalidPriority_ThrowsArgumentException()
  {
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();

    Action act = () => new ProjectTask(workspaceId, columnId, "Test Task", (TaskPriority)999, 1, null, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Invalid priority value");
  }

  [Fact]
  public void Constructor_WithAllPriorityValues_WorksCorrectly()
  {
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();

    var lowPriorityTask = new ProjectTask(workspaceId, columnId, "Low Task", TaskPriority.Low, 1, null, null);
    var mediumPriorityTask = new ProjectTask(workspaceId, columnId, "Medium Task", TaskPriority.Medium, 2, null, null);
    var highPriorityTask = new ProjectTask(workspaceId, columnId, "High Task", TaskPriority.High, 3, null, null);

    lowPriorityTask.Priority.Should().Be(TaskPriority.Low);
    mediumPriorityTask.Priority.Should().Be(TaskPriority.Medium);
    highPriorityTask.Priority.Should().Be(TaskPriority.High);
  }

  [Fact]
  public void UpdateDetails_WithValidData_UpdatesSuccessfully()
  {
    var task = new ProjectTask(Guid.NewGuid(), Guid.NewGuid(), "Original Title", TaskPriority.Low, 1, "Original desc", null);

    task.UpdateDetails("Updated Title", "Updated description", DateTime.UtcNow.AddDays(3), TaskPriority.High);

    task.Title.Should().Be("Updated Title");
    task.Description.Should().Be("Updated description");
    task.Priority.Should().Be(TaskPriority.High);
    task.DueDate.Should().NotBeNull();
    task.UpdatedAt.Should().NotBeNull();
  }

  [Fact]
  public void UpdateDetails_WithInvalidPriority_ThrowsArgumentException()
  {
    var task = new ProjectTask(Guid.NewGuid(), Guid.NewGuid(), "Test Task", TaskPriority.Low, 1, null, null);

    Action act = () => task.UpdateDetails("Updated Title", null, null, (TaskPriority)999);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Invalid priority value");
  }

  [Fact]
  public void UpdatePosition_WithValidPosition_UpdatesSuccessfully()
  {
    var task = new ProjectTask(Guid.NewGuid(), Guid.NewGuid(), "Test Task", TaskPriority.Low, 1, null, null);

    task.UpdatePosition(5);

    task.Position.Should().Be(5);
    task.UpdatedAt.Should().NotBeNull();
  }

  [Fact]
  public void MoveToColumn_WithValidData_UpdatesSuccessfully()
  {
    var originalColumnId = Guid.NewGuid();
    var task = new ProjectTask(Guid.NewGuid(), originalColumnId, "Test Task", TaskPriority.Low, 1, null, null);
    var newColumnId = Guid.NewGuid();

    task.MoveToColumn(newColumnId, 3);

    task.ColumnId.Should().Be(newColumnId);
    task.Position.Should().Be(3);
    task.UpdatedAt.Should().NotBeNull();
  }

  [Fact]
  public void MoveToColumn_WithEmptyColumnId_ThrowsArgumentException()
  {
    var task = new ProjectTask(Guid.NewGuid(), Guid.NewGuid(), "Test Task", TaskPriority.Low, 1, null, null);

    Action act = () => task.MoveToColumn(Guid.Empty, 2);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Valid column ID is required for task movement");
  }

  [Fact]
  public void UpdateDetails_ShouldUpdateUpdatedAt_WhenDataIsValid()
  {
    var task = new ProjectTask(Guid.NewGuid(), Guid.NewGuid(), "Test Task", TaskPriority.Low, 1, null, null);

    task.UpdateDetails("Updated Title 1", null, null, TaskPriority.Medium);
    var firstUpdatedAt = task.UpdatedAt;

    task.UpdateDetails("Updated Title 2", null, null, TaskPriority.High);
    var secondUpdatedAt = task.UpdatedAt;

    firstUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().BeOnOrAfter(firstUpdatedAt!.Value);
  }

  [Fact]
  public void MoveToColumn_ShouldUpdateUpdatedAt_WhenDataIsValid()
  {
    var task = new ProjectTask(Guid.NewGuid(), Guid.NewGuid(), "Test Task", TaskPriority.Low, 1, null, null);

    task.MoveToColumn(Guid.NewGuid(), 2);
    var firstUpdatedAt = task.UpdatedAt;

    task.MoveToColumn(Guid.NewGuid(), 3);
    var secondUpdatedAt = task.UpdatedAt;

    firstUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().BeOnOrAfter(firstUpdatedAt!.Value);
  }
}
