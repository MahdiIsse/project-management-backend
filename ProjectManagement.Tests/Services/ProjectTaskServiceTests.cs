using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagement.Domain;
using ProjectManagement.Application.DTOs.ProjectTasks;
using ProjectManagement.Application.Services;
using ProjectManagement.Infrastructure.Exceptions;
using ProjectManagement.Infrastructure.Mappings;
using ProjectManagement.Application.Interfaces.Repositories;

namespace ProjectManagement.Tests.Services;

public class ProjectTaskServiceTests
{
  private readonly Mock<IProjectTaskRepository> _projectTaskRepository;
  private readonly Mock<IWorkspaceRepository> _workspaceRepository;
  private readonly Mock<IColumnRepository> _columnRepository;
  private readonly Mock<IAssigneeRepository> _assigneeRepository;
  private readonly Mock<ITagRepository> _tagRepository;
  private readonly Mock<ILogger<ProjectTaskService>> _logger;
  private readonly IMapper _mapper;
  private readonly ProjectTaskService _projectTaskService;

  public ProjectTaskServiceTests()
  {
    _projectTaskRepository = new Mock<IProjectTaskRepository>();
    _workspaceRepository = new Mock<IWorkspaceRepository>();
    _columnRepository = new Mock<IColumnRepository>();
    _assigneeRepository = new Mock<IAssigneeRepository>();
    _tagRepository = new Mock<ITagRepository>();
    _logger = new Mock<ILogger<ProjectTaskService>>();

    var config = new MapperConfiguration(cfg =>
    {
      cfg.AddProfile<AutoMapperProfiles>();
    });
    _mapper = config.CreateMapper();

    _projectTaskService = new ProjectTaskService(
        _projectTaskRepository.Object,
        _workspaceRepository.Object,
        _columnRepository.Object,
        _assigneeRepository.Object,
        _tagRepository.Object,
        _mapper,
        _logger.Object
    );
  }

  [Fact]
  public async Task GetAllByWorkspaceIdAsync_WithValidData_ShouldReturnMappedTasks()
  {
    var userId = "user123";
    var workspaceId = Guid.NewGuid();
    var tasks = new List<ProjectTask>
        {
            CreateTestTask("Task 1", workspaceId, Guid.NewGuid(), userId),
            CreateTestTask("Task 2", workspaceId, Guid.NewGuid(), userId)
        };

    _projectTaskRepository.Setup(r => r.GetAllByWorkspaceIdAsync(workspaceId))
        .ReturnsAsync(tasks);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);

    var result = await _projectTaskService.GetAllByWorkspaceIdAsync(workspaceId, userId);

    result.Should().NotBeNull();
    result.Should().HaveCount(2);
    result.First().Title.Should().Be("Task 1");
    result.Last().Title.Should().Be("Task 2");

    _projectTaskRepository.Verify(r => r.GetAllByWorkspaceIdAsync(workspaceId), Times.Once);
    _projectTaskRepository.Verify(r => r.GetAllByWorkspaceIdAsync(It.IsAny<Guid>()), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithValidTask_ShouldReturnMappedTask()
  {
    var userId = "user123";
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var taskId = Guid.NewGuid();
    var task = CreateTestTask("Test Task", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);

    var result = await _projectTaskService.GetByIdAsync(taskId, userId);

    result.Should().NotBeNull();
    result.Id.Should().Be(taskId);
    result.Title.Should().Be("Test Task");

    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
  }

  [Fact]
  public async Task GetByIdAsync_WithNonExistentTask_ShouldThrowNotFoundException()
  {
    var userId = "user123";
    var taskId = Guid.NewGuid();

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync((ProjectTask?)null);

    await Assert.ThrowsAsync<NotFoundException>(() =>
        _projectTaskService.GetByIdAsync(taskId, userId));

    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
  }

  [Fact]
  public async Task CreateAsync_WithValidData_ShouldReturnCreatedTask()
  {
    var userId = "user123";
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var createDto = CreateTestCreateDto("New Task", "Description", TaskPriority.Medium);
    var task = CreateTestTask("New Task", workspaceId, columnId, userId);

    _projectTaskRepository.Setup(r => r.GetMaxPositionAsync(columnId))
.ReturnsAsync(2);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _columnRepository.Setup(r => r.ColumnExistsInWorkspaceAsync(columnId, workspaceId))
        .ReturnsAsync(true);
    _projectTaskRepository.Setup(r => r.CreateAsync(It.IsAny<ProjectTask>()))
        .ReturnsAsync(task);

    var result = await _projectTaskService.CreateAsync(workspaceId, columnId, createDto, userId);

    result.Should().NotBeNull();
    result.Title.Should().Be("New Task");
    result.Description.Should().Be("Test description");
    result.Priority.Should().Be(TaskPriority.Medium);

    _projectTaskRepository.Verify(r => r.GetMaxPositionAsync(columnId), Times.Once);
    _projectTaskRepository.Verify(r => r.CreateAsync(It.IsAny<ProjectTask>()), Times.Once);
  }

  [Fact]
  public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedTask()
  {
    var userId = "user123";
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var taskId = Guid.NewGuid();
    var task = CreateTestTask("Original Task", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    var updateDto = CreateTestUpdateDto("Updated Task", "Updated Description", TaskPriority.High, 1);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _projectTaskRepository.Setup(r => r.UpdateAsync(It.IsAny<ProjectTask>()))
        .ReturnsAsync(task);

    var result = await _projectTaskService.UpdateAsync(taskId, updateDto, userId);

    result.Should().NotBeNull();
    result.Title.Should().Be("Updated Task");
    result.Description.Should().Be("Updated Description");
    result.Priority.Should().Be(TaskPriority.High);

    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    _projectTaskRepository.Verify(r => r.UpdateAsync(It.IsAny<ProjectTask>()), Times.Once);
  }

  [Fact]
  public async Task DeleteAsync_WithValidTask_ShouldDeleteSuccessfully()
  {
    var userId = "user123";
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var taskId = Guid.NewGuid();
    var task = CreateTestTask("Task to Delete", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _projectTaskRepository.Setup(r => r.DeleteAsync(It.IsAny<ProjectTask>()))
        .Returns(Task.CompletedTask);

    await _projectTaskService.DeleteAsync(taskId, userId);

    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    _projectTaskRepository.Verify(r => r.DeleteAsync(It.IsAny<ProjectTask>()), Times.Once);
  }

  [Fact]
  public async Task AddAssigneeToTaskAsync_WithValidData_ShouldReturnUpdatedTask()
  {
    var userId = "user123";
    var taskId = Guid.NewGuid();
    var assigneeId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var task = CreateTestTask("Task with Assignee", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(new Assignee("Test Assignee", userId, null));
    _projectTaskRepository.Setup(r => r.AddAssigneeToTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Assignee>()))
        .ReturnsAsync(task);

    var result = await _projectTaskService.AddAssigneeToTaskAsync(taskId, assigneeId, userId);

    result.Should().NotBeNull();
    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    _projectTaskRepository.Verify(r => r.AddAssigneeToTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Assignee>()), Times.Once);
  }

  [Fact]
  public async Task RemoveAssigneeFromTaskAsync_WithValidData_ShouldReturnUpdatedTask()
  {
    var userId = "user123";
    var taskId = Guid.NewGuid();
    var assigneeId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var task = CreateTestTask("Task without Assignee", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _assigneeRepository.Setup(r => r.GetByIdAsync(assigneeId))
        .ReturnsAsync(new Assignee("Test Assignee", userId, null));
    _projectTaskRepository.Setup(r => r.RemoveAssigneeFromTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Assignee>()))
        .ReturnsAsync(task);

    var result = await _projectTaskService.RemoveAssigneeFromTaskAsync(taskId, assigneeId, userId);

    result.Should().NotBeNull();
    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    _projectTaskRepository.Verify(r => r.RemoveAssigneeFromTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Assignee>()), Times.Once);
  }

  [Fact]
  public async Task AddTagToTaskAsync_WithValidData_ShouldReturnUpdatedTask()
  {
    var userId = "user123";
    var taskId = Guid.NewGuid();
    var tagId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var task = CreateTestTask("Task with Tag", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(new Tag("Test Tag", "#FF5733", userId));
    _projectTaskRepository.Setup(r => r.AddTagToTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Tag>()))
        .ReturnsAsync(task);

    var result = await _projectTaskService.AddTagToTaskAsync(taskId, tagId, userId);

    result.Should().NotBeNull();
    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    _projectTaskRepository.Verify(r => r.AddTagToTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Tag>()), Times.Once);
  }

  [Fact]
  public async Task RemoveTagFromTaskAsync_WithValidData_ShouldReturnUpdatedTask()
  {
    var userId = "user123";
    var taskId = Guid.NewGuid();
    var tagId = Guid.NewGuid();
    var workspaceId = Guid.NewGuid();
    var columnId = Guid.NewGuid();
    var task = CreateTestTask("Task without Tag", workspaceId, columnId, userId);
    typeof(ProjectTask).GetProperty("Id")!.SetValue(task, taskId);

    _projectTaskRepository.Setup(r => r.GetByIdAsync(taskId))
        .ReturnsAsync(task);
    _workspaceRepository.Setup(r => r.UserHasAccessToWorkspaceAsync(workspaceId, userId))
        .ReturnsAsync(true);
    _tagRepository.Setup(r => r.GetByIdAsync(tagId))
        .ReturnsAsync(new Tag("Test Tag", "#FF5733", userId));
    _projectTaskRepository.Setup(r => r.RemoveTagFromTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Tag>()))
        .ReturnsAsync(task);

    var result = await _projectTaskService.RemoveTagFromTaskAsync(taskId, tagId, userId);

    result.Should().NotBeNull();
    _projectTaskRepository.Verify(r => r.GetByIdAsync(taskId), Times.Once);
    _projectTaskRepository.Verify(r => r.RemoveTagFromTaskAsync(It.IsAny<ProjectTask>(), It.IsAny<Tag>()), Times.Once);
  }

  private static ProjectTask CreateTestTask(string title, Guid workspaceId, Guid columnId, string userId)
  {
    return new ProjectTask(
        workspaceId: workspaceId,
        columnId: columnId,
        title: title,
        priority: TaskPriority.Medium,
        position: 1,
        description: "Test description",
        dueDate: DateTime.UtcNow.AddDays(7)
    );
  }

  private static CreateProjectTaskDto CreateTestCreateDto(string title, string? description, TaskPriority priority)
  {
    return new CreateProjectTaskDto
    {
      Title = title,
      Description = description,
      Priority = priority,
      DueDate = DateTime.UtcNow.AddDays(7)
    };
  }

  private static UpdateProjectTaskDto CreateTestUpdateDto(string title, string? description, TaskPriority priority, int position)
  {
    return new UpdateProjectTaskDto
    {
      Title = title,
      Description = description,
      Priority = priority,
      Position = position,
      DueDate = DateTime.UtcNow.AddDays(7)
    };
  }
}
