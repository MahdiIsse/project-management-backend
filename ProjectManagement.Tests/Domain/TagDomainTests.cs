using FluentAssertions;
using ProjectManagement.Domain;

namespace ProjectManagement.Tests.Domain;

public class TagDomainTests
{
  [Fact]
  public void Constructor_WithValidData_CreatesSuccessfully()
  {
    var name = "Urgent";
    var color = "#FF0000";
    var userId = "user123";

    var tag = new Tag(name, color, userId);

    tag.Should().NotBeNull();
    tag.Id.Should().NotBeEmpty();
    tag.Name.Should().Be(name);
    tag.Color.Should().Be(color);
    tag.UserId.Should().Be(userId);
    tag.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    tag.UpdatedAt.Should().BeNull();
    tag.Tasks.Should().NotBeNull();
    tag.Tasks.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WithEmptyName_ThrowsArgumentException()
  {
    var emptyName = "";
    var color = "#FF0000";
    var userId = "user123";

    Action act = () => new Tag(emptyName, color, userId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag name cannot be empty");
  }

  [Fact]
  public void Constructor_WithWhitespaceName_ThrowsArgumentException()
  {
    var whitespaceName = "   ";
    var color = "#FF0000";
    var userId = "user123";

    Action act = () => new Tag(whitespaceName, color, userId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag name cannot be empty");
  }

  [Fact]
  public void Constructor_WithNullName_ThrowsArgumentException()
  {
    string? nullName = null;
    var color = "#FF0000";
    var userId = "user123";

    Action act = () => new Tag(nullName!, color, userId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag name cannot be empty");
  }

  [Fact]
  public void Constructor_WithEmptyColor_ThrowsArgumentException()
  {
    var name = "Urgent";
    var emptyColor = "";
    var userId = "user123";

    Action act = () => new Tag(name, emptyColor, userId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag color cannot be empty");
  }

  [Fact]
  public void Constructor_WithWhitespaceColor_ThrowsArgumentException()
  {
    var name = "Urgent";
    var whitespaceColor = "   ";
    var userId = "user123";

    Action act = () => new Tag(name, whitespaceColor, userId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag color cannot be empty");
  }

  [Fact]
  public void Constructor_WithNullColor_ThrowsArgumentException()
  {
    var name = "Urgent";
    string? nullColor = null;
    var userId = "user123";

    Action act = () => new Tag(name, nullColor!, userId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag color cannot be empty");
  }

  [Fact]
  public void Constructor_WithEmptyUserId_ThrowsArgumentException()
  {
    var name = "Urgent";
    var color = "#FF0000";
    var emptyUserId = "";

    Action act = () => new Tag(name, color, emptyUserId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("User ID is required for tag creation");
  }

  [Fact]
  public void Constructor_WithWhitespaceUserId_ThrowsArgumentException()
  {
    var name = "Urgent";
    var color = "#FF0000";
    var whitespaceUserId = "   ";

    Action act = () => new Tag(name, color, whitespaceUserId);

    act.Should().Throw<ArgumentException>()
        .WithMessage("User ID is required for tag creation");
  }

  [Fact]
  public void Constructor_WithNullUserId_ThrowsArgumentException()
  {
    var name = "Urgent";
    var color = "#FF0000";
    string? nullUserId = null;

    Action act = () => new Tag(name, color, nullUserId!);

    act.Should().Throw<ArgumentException>()
        .WithMessage("User ID is required for tag creation");
  }

  [Fact]
  public void Update_WithValidData_UpdatesSuccessfully()
  {
    var tag = new Tag("Urgent", "#FF0000", "user123");

    tag.Update("High Priority", "#00FF00");

    tag.Name.Should().Be("High Priority");
    tag.Color.Should().Be("#00FF00");
    tag.UpdatedAt.Should().NotBeNull();
    tag.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
  }

  [Fact]
  public void Update_WithEmptyName_ThrowsArgumentException()
  {
    var tag = new Tag("Urgent", "#FF0000", "user123");

    Action act = () => tag.Update("", "#00FF00");

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag name cannot be empty");
  }

  [Fact]
  public void Update_WithWhitespaceName_ThrowsArgumentException()
  {
    var tag = new Tag("Urgent", "#FF0000", "user123");

    Action act = () => tag.Update("   ", "#00FF00");

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag name cannot be empty");
  }

  [Fact]
  public void Update_WithEmptyColor_ThrowsArgumentException()
  {
    var tag = new Tag("Urgent", "#FF0000", "user123");

    Action act = () => tag.Update("High Priority", "");

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag color cannot be empty");
  }

  [Fact]
  public void Update_WithWhitespaceColor_ThrowsArgumentException()
  {
    var tag = new Tag("Urgent", "#FF0000", "user123");

    Action act = () => tag.Update("High Priority", "   ");

    act.Should().Throw<ArgumentException>()
        .WithMessage("Tag color cannot be empty");
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenDataIsValid()
  {
    var tag = new Tag("Test Tag", "#000000", "user123");

    tag.Update("Updated Name 1", "#FF0000");
    var firstUpdatedAt = tag.UpdatedAt;

    tag.Update("Updated Name 2", "#00FF00");
    var secondUpdatedAt = tag.UpdatedAt;

    firstUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().BeOnOrAfter(firstUpdatedAt!.Value);
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenOnlyNameIsUpdated()
  {
    var tag = new Tag("Test Tag", "#000000", "user123");

    tag.Update("Updated Name", "#000000");
    var updatedAt = tag.UpdatedAt;

    updatedAt.Should().NotBeNull();
    tag.Name.Should().Be("Updated Name");
    tag.Color.Should().Be("#000000");
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenOnlyColorIsUpdated()
  {
    var tag = new Tag("Test Tag", "#000000", "user123");

    tag.Update("Test Tag", "#FF0000");
    var updatedAt = tag.UpdatedAt;

    updatedAt.Should().NotBeNull();
    tag.Name.Should().Be("Test Tag");
    tag.Color.Should().Be("#FF0000");
  }
}
