
using AutoMapper;
using FluentAssertions;
using Moq;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.Columns;
using ProjectManagement.Application.Services;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Infrastructure.Mappings;
using ProjectManagement.Application.Interfaces.Repositories;

namespace ProjectManagement.Tests;

public class ColumnServiceTests
{
  private readonly Mock<IColumnRepository> _columnRepository;
  private readonly IMapper _mapper;
  private readonly ColumnService _columnService;

  public ColumnServiceTests()
  {
    _columnRepository = new Mock<IColumnRepository>();

    var config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<AutoMapperProfiles>();
    });
    _mapper = config.CreateMapper();

    _columnService = new ColumnService(
        _columnRepository.Object,
        _mapper
    );
  }

  [Fact]
  public async Task GetAllAsync_WithValidWorkspaceIdAndAccess_ShouldReturnMappedColumns()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var columns = new List<Column>
        {
            CreateTestColumn("Column 1", workspaceId),
            CreateTestColumn("Column 2", workspaceId)
        };

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.GetAllByWorkspaceAsync(workspaceId))
        .ReturnsAsync(columns);

    var result = await _columnService.GetAllAsync(workspaceId, userId);

    result.Should().NotBeNull();
    result.Should().HaveCount(2);

    var resultList = result.ToList();
    resultList[0].Title.Should().Be("Column 1");
    resultList[1].Title.Should().Be("Column 2");
    resultList[0].WorkspaceId.Should().Be(workspaceId);
    resultList[1].WorkspaceId.Should().Be(workspaceId);

    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
    _columnRepository.Verify(r => r.GetAllByWorkspaceAsync(workspaceId), Times.Once);
  }

  [Fact]
  public async Task GetAllAsync_WithEmptyResult_ShouldReturnEmptyCollection()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var emptyColumns = new List<Column>();

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.GetAllByWorkspaceAsync(workspaceId))
        .ReturnsAsync(emptyColumns);

    var result = await _columnService.GetAllAsync(workspaceId, userId);

    result.Should().NotBeNull();
    result.Should().BeEmpty();

    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
    _columnRepository.Verify(r => r.GetAllByWorkspaceAsync(workspaceId), Times.Once);
  }

  [Fact]
  public async Task GetAllAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _columnService.Invoking(s => s.GetAllAsync(workspaceId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to workspace {workspaceId}");

    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
    _columnRepository.Verify(r => r.GetAllByWorkspaceAsync(It.IsAny<Guid>()), Times.Never);
  }

  [Fact]
  public async Task GetByIdAsync_WithValidIdAndAccess_ShouldReturnColumnDto()
  {
    var columnId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var column = CreateTestColumn("Test Column", workspaceId);
    typeof(Column).GetProperty("Id")!.SetValue(column, columnId);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync(column);
    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);

    var result = await _columnService.GetByIdAsync(columnId, userId);

    result.Should().NotBeNull();
    result.Id.Should().Be(columnId);
    result.Title.Should().Be("Test Column");
    result.WorkspaceId.Should().Be(workspaceId);

    _columnRepository.Verify(r => r.GetByIdAsync(columnId), Times.Once);
    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var columnId = Guid.NewGuid();
    var userId = "user123";

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync((Column?)null);

    await _columnService.Invoking(s => s.GetByIdAsync(columnId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Column with ID '{columnId}' was not found.");

    _columnRepository.Verify(r => r.GetByIdAsync(columnId), Times.Once);
    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task GetByIdAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var columnId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var column = CreateTestColumn("Test Column", workspaceId);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync(column);
    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _columnService.Invoking(s => s.GetByIdAsync(columnId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to column {columnId}");

    _columnRepository.Verify(r => r.GetByIdAsync(columnId), Times.Once);
    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithValidDataAndAccess_ShouldCreateAndReturnColumnDto()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var createDto = CreateTestCreateDto("New Column", "#FF0000");
    var maxPosition = 2;

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.WorkspaceExistsAsync(workspaceId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.GetMaxPositionAsync(workspaceId))
        .ReturnsAsync(maxPosition);

    var createdColumn = CreateTestColumn("New Column", workspaceId);
    _columnRepository.Setup(r => r.CreateAsync(It.IsAny<Column>()))
        .ReturnsAsync(createdColumn);

    var result = await _columnService.CreateAsync(workspaceId, createDto, userId);

    result.Should().NotBeNull();
    result.Title.Should().Be("New Column");
    result.Color.Should().Be("#FF0000");
    result.WorkspaceId.Should().Be(workspaceId);

    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
    _columnRepository.Verify(r => r.WorkspaceExistsAsync(workspaceId), Times.Once);
    _columnRepository.Verify(r => r.GetMaxPositionAsync(workspaceId), Times.Once);
    _columnRepository.Verify(r => r.CreateAsync(It.Is<Column>(c =>
        c.Title == "New Column" &&
        c.WorkspaceId == workspaceId &&
        c.Position == maxPosition + 1
    )), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithFirstColumn_ShouldSetPositionToOne()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var createDto = CreateTestCreateDto("First Column", null);
    var maxPosition = 0;

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.WorkspaceExistsAsync(workspaceId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.GetMaxPositionAsync(workspaceId))
        .ReturnsAsync(maxPosition);

    var createdColumn = CreateTestColumn("First Column", workspaceId);
    _columnRepository.Setup(r => r.CreateAsync(It.IsAny<Column>()))
        .ReturnsAsync(createdColumn);

    await _columnService.CreateAsync(workspaceId, createDto, userId);

    _columnRepository.Verify(r => r.CreateAsync(It.Is<Column>(c =>
        c.Position == 1
    )), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithNoWorkspaceAccess_ShouldThrowForbiddenException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var createDto = CreateTestCreateDto("New Column", null);

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _columnService.Invoking(s => s.CreateAsync(workspaceId, createDto, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to workspace {workspaceId}");

    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
    _columnRepository.Verify(r => r.WorkspaceExistsAsync(It.IsAny<Guid>()), Times.Never);
    _columnRepository.Verify(r => r.GetMaxPositionAsync(It.IsAny<Guid>()), Times.Never);
    _columnRepository.Verify(r => r.CreateAsync(It.IsAny<Column>()), Times.Never);
  }

  [Fact]
  public async Task CreateAsync_WithNonExistentWorkspace_ShouldThrowNotFoundException()
  {
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var createDto = CreateTestCreateDto("New Column", null);

    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.WorkspaceExistsAsync(workspaceId))
        .ReturnsAsync(false);

    await _columnService.Invoking(s => s.CreateAsync(workspaceId, createDto, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Workspace with ID '{workspaceId}' was not found.");

    _columnRepository.Verify(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId), Times.Once);
    _columnRepository.Verify(r => r.WorkspaceExistsAsync(workspaceId), Times.Once);
    _columnRepository.Verify(r => r.GetMaxPositionAsync(It.IsAny<Guid>()), Times.Never);
    _columnRepository.Verify(r => r.CreateAsync(It.IsAny<Column>()), Times.Never);
  }

  [Fact]
  public async Task UpdateAsync_WithValidDataAndAccess_ShouldUpdateAndReturnColumnDto()
  {
    var columnId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var existingColumn = CreateTestColumn("Old Title", workspaceId);
    typeof(Column).GetProperty("Id")!.SetValue(existingColumn, columnId);

    var updateDto = CreateTestUpdateDto("Updated Title", "#00FF00", 5);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync(existingColumn);
    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.UpdateAsync(It.IsAny<Column>()))
        .ReturnsAsync(existingColumn);

    var result = await _columnService.UpdateAsync(columnId, updateDto, userId);

    result.Should().NotBeNull();
    result.Title.Should().Be("Updated Title");
    result.Color.Should().Be("#00FF00");
    result.Position.Should().Be(5);
    result.WorkspaceId.Should().Be(workspaceId);

    _columnRepository.Verify(r => r.UpdateAsync(It.Is<Column>(c =>
        c.Title == "Updated Title" &&
        c.Position == 5
    )), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var columnId = Guid.NewGuid();
    var userId = "user123";
    var updateDto = CreateTestUpdateDto("Title", null, 1);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync((Column?)null);

    await _columnService.Invoking(s => s.UpdateAsync(columnId, updateDto, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Column with ID '{columnId}' was not found.");

    _columnRepository.Verify(r => r.UpdateAsync(It.IsAny<Column>()), Times.Never);
  }

  [Fact]
  public async Task UpdateAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var columnId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var existingColumn = CreateTestColumn("Title", workspaceId);
    var updateDto = CreateTestUpdateDto("New Title", null, 1);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync(existingColumn);
    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _columnService.Invoking(s => s.UpdateAsync(columnId, updateDto, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to column {columnId}");

    _columnRepository.Verify(r => r.UpdateAsync(It.IsAny<Column>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithValidIdAndAccess_ShouldDeleteColumn()
  {
    var columnId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var column = CreateTestColumn("Test Column", workspaceId);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync(column);
    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);

    await _columnService.DeleteAsync(columnId, userId);

    _columnRepository.Verify(r => r.DeleteAsync(column), Times.Once);
  }

  [Fact]
  public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
  {
    var columnId = Guid.NewGuid();
    var userId = "user123";

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync((Column?)null);

    await _columnService.Invoking(s => s.DeleteAsync(columnId, userId))
        .Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Column with ID '{columnId}' was not found.");

    _columnRepository.Verify(r => r.DeleteAsync(It.IsAny<Column>()), Times.Never);
  }

  [Fact]
  public async Task DeleteAsync_WithNoAccess_ShouldThrowForbiddenException()
  {
    var columnId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var userId = "user123";
    var column = CreateTestColumn("Test Column", workspaceId);

    _columnRepository.Setup(r => r.GetByIdAsync(columnId))
        .ReturnsAsync(column);
    _columnRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(false);

    await _columnService.Invoking(s => s.DeleteAsync(columnId, userId))
        .Should().ThrowAsync<ForbiddenException>()
        .WithMessage($"User does not have access to column {columnId}");

    _columnRepository.Verify(r => r.DeleteAsync(It.IsAny<Column>()), Times.Never);
  }

  private static Column CreateTestColumn(string title, Guid workspaceId)
  {
    return new Column(title, "#FF0000", workspaceId);
  }

  private static CreateColumnDto CreateTestCreateDto(string title, string? color)
  {
    return new CreateColumnDto
    {
      Title = title,
      Color = color
    };
  }

  private static UpdateColumnDto CreateTestUpdateDto(string title, string? color, int position)
  {
    return new UpdateColumnDto
    {
      Title = title,
      Color = color,
      Position = position
    };
  }
}
