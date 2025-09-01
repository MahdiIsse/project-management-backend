using FluentAssertions;
using ProjectManagement.Domain;

namespace ProjectManagement.Tests.Domain;

public class ColumnDomainTests
{
  [Fact]
  public void Constructor_WithValidData_CreatesSuccessfully()
  {
    var title = "Column 1";
    var color = "#000000";
    var workspaceId = Guid.NewGuid();

    var column = new Column(title, color, workspaceId);

    column.Should().NotBeNull();
    column.Id.Should().NotBeEmpty();
    column.Title.Should().Be(title);
    column.Color.Should().Be(color);
    column.WorkspaceId.Should().Be(workspaceId);
    column.Position.Should().Be(0);
    column.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    column.UpdatedAt.Should().BeNull();
  }

  [Fact]
  public void Constructor_ShouldThrowArgumentException_WhenTitleIsEmpty()
  {
    Action act = () => new Column("", null, Guid.NewGuid());

    act.Should().Throw<ArgumentException>()
      .WithMessage("Column title cannot be empty");
  }

  [Fact]
  public void Constructor_ShouldThrowArgumentException_WhenWorkspaceIdIsEmpty()
  {
    Action act = () => new Column("Column 1", null, Guid.Empty);

    act.Should().Throw<ArgumentException>()
      .WithMessage("Valid workspace ID is required");
  }

  [Fact]
  public void Update_WithValidData_UpdatesSuccessfully()
  {
    var column = new Column("Column 1", "#000000", Guid.NewGuid());

    column.Update("Column name changed", "#000000", 1);

    column.Title.Should().Be("Column name changed");
    column.Color.Should().Be("#000000");
    column.Position.Should().Be(1);
    column.UpdatedAt.Should().NotBeNull();
  }

  [Fact]
  public void Update_ShouldThrowArgumentException_WhenTitleIsEmpty()
  {
    var column = new Column("Column 1", "#000000", Guid.NewGuid());

    Action act = () => column.Update("", "#000000", 1);

    act.Should().Throw<ArgumentException>()
      .WithMessage("Column title cannot be empty");
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenDataIsValid()
  {
    var column = new Column("Column 3", "#000000", Guid.NewGuid());

    column.Update("Column changed 1", "#000000", 2);
    var update1 = column.UpdatedAt;

    column.Update("Column changed 2", "#000000", 3);
    var update2 = column.UpdatedAt;

    update1.Should().NotBeNull();
    update2.Should().NotBeNull();
    update2.Should().BeOnOrAfter(update1!.Value);
  }

  [Fact]
  public void SetPosition_ShouldUpdatePositionAndUpdatedAt_WhenPositionIsValid()
  {
    var column = new Column("Test Column", "#000000", Guid.NewGuid());
    var beforeSetPosition = DateTime.UtcNow;

    column.SetPosition(3);

    column.Position.Should().Be(3);
    column.UpdatedAt.Should().NotBeNull();
    column.UpdatedAt.Should().BeOnOrAfter(beforeSetPosition);
  }
}