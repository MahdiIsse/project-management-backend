using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DTOs.Assignees;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssigneeController : BaseController
{
  private readonly IAssigneeService _assigneeService;
  private readonly ILogger<AssigneeController> _logger;

  public AssigneeController(IAssigneeService assigneeService, ILogger<AssigneeController> logger)
  {
    _assigneeService = assigneeService;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetAssignees()
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var assignees = await _assigneeService.GetAllByUserIdAsync(userId);
    return Ok(assignees);
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetAssignee([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var assignee = await _assigneeService.GetByIdAsync(id, userId);
    return Ok(assignee);
  }

  [HttpPost]
  public async Task<IActionResult> CreateAssignee([FromBody] CreateAssigneeDto createAssigneeDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to create assignee");
      return Unauthorized();
    }

    _logger.LogInformation("Creating assignee '{Name}' for user {UserId}",
      createAssigneeDto.Name, userId);

    var createdAssignee = await _assigneeService.CreateAsync(createAssigneeDto, userId);

    _logger.LogInformation("Created assignee {AssigneeId} with name '{Name}' for user {UserId}",
      createdAssignee.Id, createdAssignee.Name, userId);

    return CreatedAtAction(nameof(GetAssignee), new { id = createdAssignee.Id }, createdAssignee);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateAssignee([FromRoute] Guid id, [FromBody] UpdateAssigneeDto updateAssigneeDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to update assignee {AssigneeId}", id);
      return Unauthorized();
    }

    _logger.LogInformation("Updating assignee {AssigneeId} with name '{Name}' for user {UserId}",
      id, updateAssigneeDto.Name, userId);

    var updatedAssignee = await _assigneeService.UpdateAsync(id, updateAssigneeDto, userId);

    _logger.LogInformation("Updated assignee {AssigneeId} successfully for user {UserId}", id, userId);

    return Ok(updatedAssignee);
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteAssignee([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to delete assignee {AssigneeId}", id);
      return Unauthorized();
    }

    _logger.LogInformation("Deleting assignee {AssigneeId} for user {UserId}", id, userId);

    await _assigneeService.DeleteAsync(id, userId);

    _logger.LogInformation("Deleted assignee {AssigneeId} successfully for user {UserId}", id, userId);

    return NoContent();
  }
}
