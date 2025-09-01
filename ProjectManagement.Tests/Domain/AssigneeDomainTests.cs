using FluentAssertions;
using ProjectManagement.Domain;

namespace ProjectManagement.Tests.Domain;

public class AssigneeDomainTests
{
  [Fact]
  public void Constructor_WithValidData_CreatesSuccessfully()
  {
    var name = "John Doe";
    var userId = "user123";
    var avatarUrl = "https://example.com/avatar.jpg";

    var assignee = new Assignee(name, userId, avatarUrl);

    assignee.Should().NotBeNull();
    assignee.Id.Should().NotBeEmpty();
    assignee.Name.Should().Be(name);
    assignee.UserId.Should().Be(userId);
    assignee.AvatarUrl.Should().Be(avatarUrl);
    assignee.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    assignee.UpdatedAt.Should().BeNull();
    assignee.Tasks.Should().NotBeNull();
    assignee.Tasks.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WithValidDataAndNullAvatarUrl_CreatesSuccessfully()
  {
    var name = "Jane Smith";
    var userId = "user456";

    var assignee = new Assignee(name, userId, null);

    assignee.Should().NotBeNull();
    assignee.Name.Should().Be(name);
    assignee.UserId.Should().Be(userId);
    assignee.AvatarUrl.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithEmptyName_ThrowsArgumentException()
  {
    var emptyName = "";
    var userId = "user123";

    Action act = () => new Assignee(emptyName, userId, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Assignee name cannot be empty");
  }

  [Fact]
  public void Constructor_WithWhitespaceName_ThrowsArgumentException()
  {
    var whitespaceName = "   ";
    var userId = "user123";

    Action act = () => new Assignee(whitespaceName, userId, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Assignee name cannot be empty");
  }

  [Fact]
  public void Constructor_WithNullName_ThrowsArgumentException()
  {
    string? nullName = null;
    var userId = "user123";

    Action act = () => new Assignee(nullName!, userId, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("Assignee name cannot be empty");
  }

  [Fact]
  public void Constructor_WithEmptyUserId_ThrowsArgumentException()
  {
    var name = "John Doe";
    var emptyUserId = "";

    Action act = () => new Assignee(name, emptyUserId, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("User ID is required for assignee creation");
  }

  [Fact]
  public void Constructor_WithWhitespaceUserId_ThrowsArgumentException()
  {
    var name = "John Doe";
    var whitespaceUserId = "   ";

    Action act = () => new Assignee(name, whitespaceUserId, null);

    act.Should().Throw<ArgumentException>()
        .WithMessage("User ID is required for assignee creation");
  }

  [Fact]
  public void Update_WithValidData_UpdatesSuccessfully()
  {
    var assignee = new Assignee("Original Name", "user123", "old-avatar.jpg");

    assignee.Update("Updated Name", "new-avatar.jpg");

    assignee.Name.Should().Be("Updated Name");
    assignee.AvatarUrl.Should().Be("new-avatar.jpg");
    assignee.UpdatedAt.Should().NotBeNull();
    assignee.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
  }

  [Fact]
  public void Update_WithValidDataAndNullAvatarUrl_UpdatesSuccessfully()
  {
    var assignee = new Assignee("Original Name", "user123", "old-avatar.jpg");

    assignee.Update("Updated Name", null);

    assignee.Name.Should().Be("Updated Name");
    assignee.AvatarUrl.Should().BeNull();
    assignee.UpdatedAt.Should().NotBeNull();
  }

  [Fact]
  public void Update_WithEmptyName_ThrowsArgumentException()
  {
    var assignee = new Assignee("Original Name", "user123", null);

    Action act = () => assignee.Update("", "new-avatar.jpg");

    act.Should().Throw<ArgumentException>()
        .WithMessage("Assignee name cannot be empty");
  }

  [Fact]
  public void Update_WithWhitespaceName_ThrowsArgumentException()
  {
    var assignee = new Assignee("Original Name", "user123", null);

    Action act = () => assignee.Update("   ", "new-avatar.jpg");

    act.Should().Throw<ArgumentException>()
        .WithMessage("Assignee name cannot be empty");
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenDataIsValid()
  {
    var assignee = new Assignee("Test Name", "user123", null);

    assignee.Update("Updated Name 1", "avatar1.jpg");
    var firstUpdatedAt = assignee.UpdatedAt;

    assignee.Update("Updated Name 2", "avatar2.jpg");
    var secondUpdatedAt = assignee.UpdatedAt;

    firstUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().NotBeNull();
    secondUpdatedAt.Should().BeOnOrAfter(firstUpdatedAt!.Value);
  }

  [Fact]
  public void Update_ShouldUpdateUpdatedAt_WhenOnlyNameIsUpdated()
  {
    var assignee = new Assignee("Test Name", "user123", "original-avatar.jpg");

    assignee.Update("Updated Name", "original-avatar.jpg");
    var updatedAt = assignee.UpdatedAt;

    updatedAt.Should().NotBeNull();
    assignee.Name.Should().Be("Updated Name");
    assignee.AvatarUrl.Should().Be("original-avatar.jpg");
  }
}
