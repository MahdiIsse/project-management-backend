using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Workspaces;
using ProjectManagement.Application.Services;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Infrastructure.Mappings;
using ProjectManagement.Application.Interfaces.Repositories;

namespace ProjectManagement.Tests;

public class WorkspaceServiceTests
{
  private readonly Mock<IWorkspaceRepository> _workspaceRepository;
  private readonly Mock<ILogger<WorkspaceService>> _logger; private readonly IMapper _mapper;
  private readonly WorkspaceService _workspaceService;

  public WorkspaceServiceTests()
  {
    _workspaceRepository = new Mock<IWorkspaceRepository>();
    _logger = new Mock<ILogger<WorkspaceService>>();

    var config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<AutoMapperProfiles>();
    });
    _mapper = config.CreateMapper();

    _workspaceService = new WorkspaceService(
        _workspaceRepository.Object,
        _mapper,
        _logger.Object
    );
  }

  [Fact]
  public async Task GetAllAsync_WithValidUserId_ShouldReturnMappedWorkspaces()
  {
    var userId = "user123";
    var workspaces = new List<Workspace>
        {
            CreateTestWorkspace("Workspace 1", userId),
            CreateTestWorkspace("Workspace 2", userId)
        };

    _workspaceRepository.Setup(r => r.GetAllByUserIdAsync(userId))
        .ReturnsAsync(workspaces);

    var result = await _workspaceService.GetAllAsync(userId);

    result.Should().NotBeNull();
    result.Should().HaveCount(2);

    var resultList = result.ToList();
    resultList[0].Title.Should().Be("Workspace 1");
    resultList[1].Title.Should().Be("Workspace 2");

    _workspaceRepository.Verify(r => r.GetAllByUserIdAsync(userId), Times.Once);
  }

  [Fact]
  public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyCollection()
  {
    var userId = "user123";
    var emptyWorkspaces = new List<Workspace>();

    _workspaceRepository.Setup(r => r.GetAllByUserIdAsync(userId))
        .ReturnsAsync(emptyWorkspaces);

    var result = await _workspaceService.GetAllAsync(userId);

    result.Should().NotBeNull();
    result.Should().BeEmpty();

    _workspaceRepository.Verify(r => r.GetAllByUserIdAsync(userId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithValidIdAndAccess_ShouldReturnWorkspaceDto()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var workspace = CreateTestWorkspace("Test Workspace", userId);
    typeof(Workspace).GetProperty("Id")!.SetValue(workspace, workspaceId);

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync(workspace);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);

    var result = await _workspaceService.GetByIdAsync(workspaceId, userId);

    result.Should().NotBeNull();
    result.Id.Should().Be(workspaceId);
    result.Title.Should().Be("Test Workspace");

    _workspaceRepository.Verify(r => r.GetByIdAsync(workspaceId), Times.Once);
    _workspaceRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync((Workspace?)null);

    await _workspaceService.Invoking(s => s.GetByIdAsync(workspaceId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Workspace with ID '{workspaceId}' was not found.");

    _workspaceRepository.Verify(r => r.GetByIdAsync(workspaceId), Times.Once);
    _workspaceRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task GetByIdAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var workspace = CreateTestWorkspace("Test Workspace", "other-user");

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync(workspace);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _workspaceService.Invoking(s => s.GetByIdAsync(workspaceId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to workspace: {workspaceId}");

    _workspaceRepository.Verify(r => r.GetByIdAsync(workspaceId), Times.Once);
    _workspaceRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithValidData_ShouldCreateAndReturnWorkspaceDto()
  {
    var createDto = CreateTestCreateDto("New Workspace", "Description", "#FF0000");
    var userId = "user123";
    var maxPosition = 2;

    _workspaceRepository.Setup(r => r.GetMaxPositionAsync(userId))
        .ReturnsAsync(maxPosition);

    var createdWorkspace = CreateTestWorkspace("New Workspace", userId);
    _workspaceRepository.Setup(r => r.CreateAsync(It.IsAny<Workspace>()))
        .ReturnsAsync(createdWorkspace);

    var result = await _workspaceService.CreateAsync(createDto, userId);

    result.Should().NotBeNull();
    result.Title.Should().Be("New Workspace");
    result.Description.Should().Be("Description");
    result.Color.Should().Be("#FF0000");

    _workspaceRepository.Verify(r => r.GetMaxPositionAsync(userId), Times.Once);
    _workspaceRepository.Verify(r => r.CreateAsync(It.Is<Workspace>(w =>
        w.Title == "New Workspace" &&
        w.UserId == userId &&
        w.Position == maxPosition + 1
    )), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithFirstWorkspace_ShouldSetPositionToOne()
  {
    var createDto = CreateTestCreateDto("First Workspace", null, null);
    var userId = "user123";
    var maxPosition = -1;

    _workspaceRepository.Setup(r => r.GetMaxPositionAsync(userId))
        .ReturnsAsync(maxPosition);

    var createdWorkspace = CreateTestWorkspace("First Workspace", userId);
    _workspaceRepository.Setup(r => r.CreateAsync(It.IsAny<Workspace>()))
        .ReturnsAsync(createdWorkspace);

    await _workspaceService.CreateAsync(createDto, userId);

    _workspaceRepository.Verify(r => r.CreateAsync(It.Is<Workspace>(w =>
        w.Position == 0
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithValidData_ShouldUpdateAndReturnWorkspaceDto()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var existingWorkspace = CreateTestWorkspace("Old Title", userId);
    typeof(Workspace).GetProperty("Id")!.SetValue(existingWorkspace, workspaceId);

    var updateDto = CreateTestUpdateDto("Updated Title", "New Description", "#00FF00", 5);

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync(existingWorkspace);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _workspaceRepository.Setup(r => r.UpdateAsync(It.IsAny<Workspace>()))
        .ReturnsAsync(existingWorkspace);

    var result = await _workspaceService.UpdateAsync(workspaceId, updateDto, userId);

    result.Should().NotBeNull();
    result.Title.Should().Be("Updated Title");
    result.Description.Should().Be("New Description");
    result.Color.Should().Be("#00FF00");
    result.Position.Should().Be(5);

    _workspaceRepository.Verify(r => r.UpdateAsync(It.Is<Workspace>(w =>
        w.Title == "Updated Title" &&
        w.Position == 5
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var updateDto = CreateTestUpdateDto("Title", null, null, 1);

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync((Workspace?)null);

    await _workspaceService.Invoking(s => s.UpdateAsync(workspaceId, updateDto, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Workspace with ID '{workspaceId}' was not found.");

    _workspaceRepository.Verify(r => r.UpdateAsync(It.IsAny<Workspace>()), Times.Never);
  }

  [Fact]
  public async Task UpdateAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var existingWorkspace = CreateTestWorkspace("Title", "other-user");
    var updateDto = CreateTestUpdateDto("New Title", null, null, 1);

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync(existingWorkspace);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _workspaceService.Invoking(s => s.UpdateAsync(workspaceId, updateDto, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to workspace: {workspaceId}");

    _workspaceRepository.Verify(r => r.UpdateAsync(It.IsAny<Workspace>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithValidIdAndAccess_ShouldDeleteWorkspace()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var workspace = CreateTestWorkspace("Test Workspace", userId);

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync(workspace);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);

    await _workspaceService.DeleteAsync(workspaceId, userId);

    _workspaceRepository.Verify(r => r.DeleteAsync(workspace), Times.Once);
  }

  [Fact]
  public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync((Workspace?)null);

    await _workspaceService.Invoking(s => s.DeleteAsync(workspaceId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Workspace with ID '{workspaceId}' was not found.");

    _workspaceRepository.Verify(r => r.DeleteAsync(It.IsAny<Workspace>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var workspace = CreateTestWorkspace("Test Workspace", "other-user");

    _workspaceRepository.Setup(r => r.GetByIdAsync(workspaceId))
        .ReturnsAsync(workspace);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _workspaceService.Invoking(s => s.DeleteAsync(workspaceId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to workspace: {workspaceId}");

    _workspaceRepository.Verify(r => r.DeleteAsync(It.IsAny<Workspace>()), Times.Never);
  }

  private static Workspace CreateTestWorkspace(string title, string userId)
  {
    return new Workspace(title, "Description", "#FF0000", userId);
  }

  private static CreateWorkspaceDto CreateTestCreateDto(string title, string? description, string? color)
  {
    return new CreateWorkspaceDto
    {
      Title = title,
      Description = description,
      Color = color
    };
  }

  private static UpdateWorkspaceDto CreateTestUpdateDto(string title, string? description, string? color, int position)
  {
    return new UpdateWorkspaceDto
    {
      Title = title,
      Description = description,
      Color = color,
      Position = position
    };
  }
}