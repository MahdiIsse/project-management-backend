using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.Interfaces.Services;
using ProjectManagement.Application.DTOs.Workspaces;

namespace ProjectManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkspaceController : BaseController
{
  private readonly IWorkspaceService _workspaceService;
  private readonly ILogger<WorkspaceController> _logger;

  public WorkspaceController(
    IWorkspaceService workspaceService,
    ILogger<WorkspaceController> logger)
  {
    _workspaceService = workspaceService;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to get all workspaces - no user ID found");
      return Unauthorized();
    }

    _logger.LogInformation("Getting all workspaces for user {UserId}", userId);

    var workspaces = await _workspaceService.GetAllAsync(userId);

    _logger.LogInformation("Retrieved {Count} workspaces for user {UserId}",
      workspaces?.Count() ?? 0, userId);

    return Ok(workspaces);
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetById([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to get workspace {WorkspaceId} - no user ID found", id);
      return Unauthorized();
    }

    _logger.LogInformation("Getting workspace {WorkspaceId} for user {UserId}", id, userId);

    var workspace = await _workspaceService.GetByIdAsync(id, userId);

    _logger.LogInformation("Retrieved workspace {WorkspaceId} with title '{Title}' for user {UserId}",
      id, workspace?.Title ?? "Unknown", userId);

    return Ok(workspace);
  }

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto createWorkspaceDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to create workspace - no user ID found");
      return Unauthorized();
    }

    _logger.LogInformation("Creating workspace '{Title}' for user {UserId}",
      createWorkspaceDto.Title, userId);

    var createdWorkspace = await _workspaceService.CreateAsync(createWorkspaceDto, userId);

    _logger.LogInformation("Created workspace {WorkspaceId} with title '{Title}' for user {UserId}",
      createdWorkspace.Id, createdWorkspace.Title, userId);

    return CreatedAtAction(nameof(GetById), new { id = createdWorkspace.Id }, createdWorkspace);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateWorkspaceDto updateWorkspaceDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to update workspace {WorkspaceId} - no user ID found", id);
      return Unauthorized();
    }

    _logger.LogInformation("Updating workspace {WorkspaceId} with title '{Title}' for user {UserId}",
      id, updateWorkspaceDto.Title, userId);

    var updatedWorkspace = await _workspaceService.UpdateAsync(id, updateWorkspaceDto, userId);

    _logger.LogInformation("Updated workspace {WorkspaceId} successfully for user {UserId}",
      id, userId);

    return Ok(updatedWorkspace);
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> Delete([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to delete workspace {WorkspaceId} - no user ID found", id);
      return Unauthorized();
    }

    _logger.LogInformation("Deleting workspace {WorkspaceId} for user {UserId}", id, userId);

    await _workspaceService.DeleteAsync(id, userId);

    _logger.LogInformation("Deleted workspace {WorkspaceId} successfully for user {UserId}",
      id, userId);

    return NoContent();
  }
}