using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DTOs.ProjectTasks;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[Route("api/workspaces/{workspaceId:guid}/tasks")]
[ApiController]
public class TasksController : BaseController
{
  private readonly IProjectTaskService _projectTaskService;
  private readonly ILogger<TasksController> _logger;

  public TasksController(IProjectTaskService projectTaskService, ILogger<TasksController> logger)
  {
    _projectTaskService = projectTaskService;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetTasks([FromRoute] Guid workspaceId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var tasks = await _projectTaskService.GetAllByWorkspaceIdAsync(workspaceId, userId);
    return Ok(tasks);
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetTaskById([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var task = await _projectTaskService.GetByIdAsync(id, userId);
    return Ok(task);
  }

  [HttpPost]
  public async Task<IActionResult> CreateTask(
    [FromRoute] Guid workspaceId,
    [FromQuery] Guid columnId,
    [FromBody] CreateProjectTaskDto dto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to create task in workspace {WorkspaceId}, column {ColumnId}", workspaceId, columnId);
      return Unauthorized();
    }

    _logger.LogInformation("Creating task '{Title}' in workspace {WorkspaceId}, column {ColumnId} for user {UserId}",
      dto.Title, workspaceId, columnId, userId);

    var createdTask = await _projectTaskService.CreateAsync(workspaceId, columnId, dto, userId);

    _logger.LogInformation("Created task {TaskId} with title '{Title}' for user {UserId}",
      createdTask.Id, createdTask.Title, userId);

    return CreatedAtAction(nameof(GetTaskById), new { workspaceId, id = createdTask.Id }, createdTask);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateTask([FromRoute] Guid id, [FromBody] UpdateProjectTaskDto updateProjectTaskDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to update task {TaskId}", id);
      return Unauthorized();
    }

    _logger.LogInformation("Updating task {TaskId} with title '{Title}' for user {UserId}",
      id, updateProjectTaskDto.Title, userId);

    var updatedTask = await _projectTaskService.UpdateAsync(id, updateProjectTaskDto, userId);

    _logger.LogInformation("Updated task {TaskId} successfully for user {UserId}", id, userId);

    return Ok(updatedTask);
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteTask([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to delete task {TaskId}", id);
      return Unauthorized();
    }

    _logger.LogInformation("Deleting task {TaskId} for user {UserId}", id, userId);

    await _projectTaskService.DeleteAsync(id, userId);

    _logger.LogInformation("Deleted task {TaskId} successfully for user {UserId}", id, userId);

    return NoContent();
  }

  [HttpPost("{taskId:guid}/assignees/{assigneeId:guid}")]
  public async Task<IActionResult> AddAssigneeToTask([FromRoute] Guid taskId, [FromRoute] Guid assigneeId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var updatedTask = await _projectTaskService.AddAssigneeToTaskAsync(taskId, assigneeId, userId);
    return Ok(updatedTask);
  }

  [HttpDelete("{taskId:guid}/assignees/{assigneeId:guid}")]
  public async Task<IActionResult> RemoveAssigneeFromTask([FromRoute] Guid taskId, [FromRoute] Guid assigneeId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var updatedTask = await _projectTaskService.RemoveAssigneeFromTaskAsync(taskId, assigneeId, userId);
    return Ok(updatedTask);
  }

  [HttpPost("{taskId:guid}/tags/{tagId:guid}")]
  public async Task<IActionResult> AddTagToTask([FromRoute] Guid taskId, [FromRoute] Guid tagId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var updatedTask = await _projectTaskService.AddTagToTaskAsync(taskId, tagId, userId);
    return Ok(updatedTask);
  }

  [HttpDelete("{taskId:guid}/tags/{tagId:guid}")]
  public async Task<IActionResult> RemoveTagFromTask([FromRoute] Guid taskId, [FromRoute] Guid tagId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var updatedTask = await _projectTaskService.RemoveTagFromTaskAsync(taskId, tagId, userId);
    return Ok(updatedTask);
  }
}