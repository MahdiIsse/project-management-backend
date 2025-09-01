using FluentAssertions;
using ProjectManagement.Domain;

namespace ProjectManagement.Tests.Domain;

public class WorkspaceDomainTests
{
  [Fact]
  public void Constructor_WithValidData_CreatesSuccessfully()
  {
    var title = "My Workspace";
    var description = "Workspace description";
    var color = "#FF0000";
    var userId = "user123";

    var workspace = new Workspace(title, description, color, userId);

    workspace.Should().NotBeNull();
    workspace.Id.Should().NotBeEmpty();
    workspace.Title.Should().Be(title);
    workspace.Description.Should().Be(description);
    workspace.Color.Should().Be(color);
    workspace.UserId.Should().Be(userId);
    workspace.Position.Should().Be(0);
    workspace.UpdatedAt.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithEmptyTitle_ThrowsArgumentException()
  {
    var emptyTitle = "";
    var userId = "user123";

    Action act = () => new Workspace(emptyTitle, null, null, userId);

    act.Should().Throw<ArgumentException>()
      .WithMessage("Workspace title cannot be empty");
  }

  [Fact]
  public void Constructor_WithEmptyUserId_ThrowsArgumentException()
  {
    Action act = () => new Workspace("Workspace 1", null, null, "");

    act.Should().Throw<ArgumentException>()
      .WithMessage("User ID is required for workspace creation");
  }

  [Fact]
  public void Update_WithValidData_UpdatesSuccessfully()
  {
    var workspace = new Workspace("Title", null, null, "user123");

    workspace.Update("Updated", "New Description", "#00FF00", 2);

    workspace.Title.Should().Be("Updated");
    workspace.Description.Should().Be("New Description");
    workspace.Color.Should().Be("#00FF00");
    workspace.Position.Should().Be(2);
    workspace.UpdatedAt.Should().NotBeNull();
    workspace.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
  }

  [Fact]
  public void Update_WithEmptyTitle_ThrowsArgumentException()
  {
    var workspace = new Workspace("Title", null, null, "user123");

    Action act = () => workspace.Update("", null, null, 3);

    act.Should().Throw<ArgumentException>()
      .WithMessage("Workspace title cannot be empty");
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenDataIsValid()
  {
    var workspace = new Workspace("Workspace 2", null, null, "user123");

    workspace.Update("Workspace 2", null, null, 3);
    var firstUpdatedAt = workspace.UpdatedAt;

    workspace.Update("Workspace 2", null, null, 4);
    var secondUpdatedAt = workspace.UpdatedAt;

    firstUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().BeOnOrAfter(firstUpdatedAt!.Value);
  }

  [Fact]
  public void SetPosition_ShouldUpdatePositionAndUpdatedAt_WhenPositionIsValid()
  {
    var workspace = new Workspace("Test Workspace", null, null, "user123");
    var beforeSetPosition = DateTime.UtcNow;

    workspace.SetPosition(6);

    workspace.Position.Should().Be(6);
    workspace.UpdatedAt.Should().NotBeNull();
    workspace.UpdatedAt.Should().BeOnOrAfter(beforeSetPosition);
  }
}