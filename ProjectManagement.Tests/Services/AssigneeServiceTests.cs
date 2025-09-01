using AutoMapper;
using FluentAssertions;
using Moq;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Application.Services;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Infrastructure.Mappings;
using ProjectManagement.Application.Interfaces.Repositories;


namespace ProjectManagement.Tests.Services;

public class AssigneeServiceTests
{
  private readonly Mock<IAssigneeRepository> _assigneeRepository;
  private readonly IMapper _mapper;
  private readonly AssigneeService _assigneeService;

  public AssigneeServiceTests()
  {
    _assigneeRepository = new Mock<IAssigneeRepository>();

    var config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<AutoMapperProfiles>();
    });
    _mapper = config.CreateMapper();

    _assigneeService = new AssigneeService(
        _assigneeRepository.Object,
        _mapper
    );
  }

  [Fact]
  public async Task GetAllByUserIdAsync_WithValidUserId_ShouldReturnMappedAssignees()
  {
    var userId = "user123";
    var assignees = new List<Assignee>
        {
            CreateTestAssignee("Assignee 1", userId, "avatar1.jpg"),
            CreateTestAssignee("Assignee 2", userId, null)
        };

    _assigneeRepository.Setup(r => r.GetAllByUserIdAsync(userId))
        .ReturnsAsync(assignees);

    var result = await _assigneeService.GetAllByUserIdAsync(userId);

    result.Should().NotBeNull();
    result.Should().HaveCount(2);

    var resultList = result.ToList();
    resultList[0].Name.Should().Be("Assignee 1");
    resultList[0].AvatarUrl.Should().Be("avatar1.jpg");
    resultList[1].Name.Should().Be("Assignee 2");
    resultList[1].AvatarUrl.Should().BeNull();

    _assigneeRepository.Verify(r => r.GetAllByUserIdAsync(userId), Times.Once);
  }

  [Fact]
  public async Task GetAllByUserIdAsync_WithEmptyResult_ShouldReturnEmptyCollection()
  {
    var userId = "user123";
    var emptyAssignees = new List<Assignee>();

    _assigneeRepository.Setup(r => r.GetAllByUserIdAsync(userId))
        .ReturnsAsync(emptyAssignees);

    var result = await _assigneeService.GetAllByUserIdAsync(userId);

    result.Should().NotBeNull();
    result.Should().BeEmpty();

    _assigneeRepository.Verify(r => r.GetAllByUserIdAsync(userId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithValidIdAndAccess_ShouldReturnAssigneeDto()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var assignee = CreateTestAssignee("Test Assignee", userId, "avatar.jpg");
    typeof(Assignee).GetProperty("Id")!.SetValue(assignee, assigneeId);

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(assignee);

    var result = await _assigneeService.GetByIdAsync(assigneeId, userId);

    result.Should().NotBeNull();
    result.Id.Should().Be(assigneeId);
    result.Name.Should().Be("Test Assignee");
    result.AvatarUrl.Should().Be("avatar.jpg");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync((Assignee?)null);

    await _assigneeService.Invoking(s => s.GetByIdAsync(assigneeId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Assignee with ID '{assigneeId}' was not found.");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var differentUserId = "differentUser";
    var assignee = CreateTestAssignee("Test Assignee", differentUserId, "avatar.jpg");

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(assignee);

    await _assigneeService.Invoking(s => s.GetByIdAsync(assigneeId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to assignee: {assigneeId}");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithValidData_ShouldCreateAndReturnAssigneeDto()
  {
    var userId = "user123";
    var createDto = CreateTestCreateDto("New Assignee", "newavatar.jpg");

    var createdAssignee = CreateTestAssignee("New Assignee", userId, "newavatar.jpg");
    _assigneeRepository.Setup(r => r.CreateAsync(It.IsAny<Assignee>()))
        .ReturnsAsync(createdAssignee);

    var result = await _assigneeService.CreateAsync(createDto, userId);

    result.Should().NotBeNull();
    result.Name.Should().Be("New Assignee");
    result.AvatarUrl.Should().Be("newavatar.jpg");

    _assigneeRepository.Verify(r => r.CreateAsync(It.Is<Assignee>(a =>
        a.Name == "New Assignee" &&
        a.UserId == userId &&
        a.AvatarUrl == "newavatar.jpg"
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithValidDataAndAccess_ShouldUpdateAndReturnAssigneeDto()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var existingAssignee = CreateTestAssignee("Old Name", userId, "oldavatar.jpg");
    typeof(Assignee).GetProperty("Id")!.SetValue(existingAssignee, assigneeId);

    var updateDto = CreateTestUpdateDto("Updated Name", "updatedavatar.jpg");

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(existingAssignee);
    _assigneeRepository.Setup(r => r.UpdateAsync(It.IsAny<Assignee>()))
        .ReturnsAsync(existingAssignee);

    var result = await _assigneeService.UpdateAsync(assigneeId, updateDto, userId);

    result.Should().NotBeNull();
    result.Name.Should().Be("Updated Name");
    result.AvatarUrl.Should().Be("updatedavatar.jpg");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
    _assigneeRepository.Verify(r => r.UpdateAsync(It.Is<Assignee>(a =>
        a.Name == "Updated Name" &&
        a.AvatarUrl == "updatedavatar.jpg"
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var updateDto = CreateTestUpdateDto("Updated Name", null);

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync((Assignee?)null);

    await _assigneeService.Invoking(s => s.UpdateAsync(assigneeId, updateDto, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Assignee with ID '{assigneeId}' was not found.");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
    _assigneeRepository.Verify(r => r.UpdateAsync(It.IsAny<Assignee>()), Times.Never);
  }

  [Fact]
  public async Task UpdateAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var differentUserId = "differentUser";
    var existingAssignee = CreateTestAssignee("Test Assignee", differentUserId, "avatar.jpg");
    var updateDto = CreateTestUpdateDto("New Name", "newavatar.jpg");

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(existingAssignee);

    await _assigneeService.Invoking(s => s.UpdateAsync(assigneeId, updateDto, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to assignee: {assigneeId}");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
    _assigneeRepository.Verify(r => r.UpdateAsync(It.IsAny<Assignee>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithValidIdAndAccess_ShouldDeleteAssignee()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var assignee = CreateTestAssignee("Test Assignee", userId, "avatar.jpg");

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(assignee);

    await _assigneeService.DeleteAsync(assigneeId, userId);

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
    _assigneeRepository.Verify(r => r.DeleteAsync(assignee), Times.Once);
  }

  [Fact]
  public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync((Assignee?)null);

    await _assigneeService.Invoking(s => s.DeleteAsync(assigneeId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Assignee with ID '{assigneeId}' was not found.");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
    _assigneeRepository.Verify(r => r.DeleteAsync(It.IsAny<Assignee>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var assigneeId = Guid.NewGuid();
    var userId = "user123";
    var differentUserId = "differentUser";
    var assignee = CreateTestAssignee("Test Assignee", differentUserId, "avatar.jpg");

    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(assignee);

    await _assigneeService.Invoking(s => s.DeleteAsync(assigneeId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to assignee: {assigneeId}");

    _assigneeRepository.Verify(r => r.GetByIdAsync(assigneeId), Times.Once);
    _assigneeRepository.Verify(r => r.DeleteAsync(It.IsAny<Assignee>()), Times.Never);
  }

  private static Assignee CreateTestAssignee(string name, string userId, string? avatarUrl)
  {
    return new Assignee(name, userId, avatarUrl);
  }

  private static CreateAssigneeDto CreateTestCreateDto(string name, string? avatarUrl)
  {
    return new CreateAssigneeDto
    {
      Name = name,
      AvatarUrl = avatarUrl
    };
  }

  private static UpdateAssigneeDto CreateTestUpdateDto(string name, string? avatarUrl)
  {
    return new UpdateAssigneeDto
    {
      Name = name,
      AvatarUrl = avatarUrl
    };
  }
}
