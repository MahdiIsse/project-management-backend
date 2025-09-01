using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DTOs.Columns;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[Route("api/workspaces/{workspaceId:guid}/columns")]
[ApiController]
public class ColumnController : BaseController
{
  private readonly IColumnService _columnService;
  private readonly ILogger<ColumnController> _logger;

  public ColumnController(IColumnService columnService, ILogger<ColumnController> logger)
  {
    _columnService = columnService;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetWorkspaceColumns([FromRoute] Guid workspaceId)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to get columns for workspace {WorkspaceId}", workspaceId);
      return Unauthorized();
    }

    _logger.LogInformation("Getting columns for workspace {WorkspaceId} by user {UserId}", workspaceId, userId);

    var columns = await _columnService.GetAllAsync(workspaceId, userId);

    _logger.LogInformation("Retrieved {Count} columns for workspace {WorkspaceId}", columns?.Count() ?? 0, workspaceId);

    return Ok(columns);
  }

  [HttpGet("{columnId:guid}")]
  public async Task<IActionResult> GetColumnById([FromRoute] Guid columnId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var column = await _columnService.GetByIdAsync(columnId, userId);
    return Ok(column);
  }

  [HttpPost]
  public async Task<IActionResult> CreateColumn([FromRoute] Guid workspaceId, [FromBody] CreateColumnDto createColumnDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to create column in workspace {WorkspaceId}", workspaceId);
      return Unauthorized();
    }

    _logger.LogInformation("Creating column '{Title}' in workspace {WorkspaceId} for user {UserId}",
      createColumnDto.Title, workspaceId, userId);

    var createdColumn = await _columnService.CreateAsync(workspaceId, createColumnDto, userId);

    _logger.LogInformation("Created column {ColumnId} with title '{Title}' in workspace {WorkspaceId}",
      createdColumn.Id, createdColumn.Title, workspaceId);

    return CreatedAtAction(nameof(GetColumnById), new { workspaceId, columnId = createdColumn.Id }, createdColumn);
  }

  [HttpPut("{columnId:guid}")]
  public async Task<IActionResult> UpdateColumn([FromRoute] Guid columnId, [FromBody] UpdateColumnDto updateColumnDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var updatedColumn = await _columnService.UpdateAsync(columnId, updateColumnDto, userId);
    return Ok(updatedColumn);
  }

  [HttpDelete("{columnId:guid}")]
  public async Task<IActionResult> DeleteColumn(Guid columnId)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    await _columnService.DeleteAsync(columnId, userId);
    return NoContent();
  }
}