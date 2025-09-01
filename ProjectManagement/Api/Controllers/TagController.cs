using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DTOs.Tags;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagController : BaseController
{
  private readonly ITagService _tagService;
  private readonly ILogger<TagController> _logger;

  public TagController(ITagService tagService, ILogger<TagController> logger)
  {
    _tagService = tagService;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetAllTags()
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var tags = await _tagService.GetAllByUserIdAsync(userId);
    return Ok(tags);
  }

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetTag([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var tag = await _tagService.GetByIdAsync(id, userId);
    return Ok(tag);
  }

  [HttpPost]
  public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createTagDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      _logger.LogWarning("Unauthorized attempt to create tag");
      return Unauthorized();
    }

    _logger.LogInformation("Creating tag '{Name}' with color '{Color}' for user {UserId}",
      createTagDto.Name, createTagDto.Color, userId);

    var createdTag = await _tagService.CreateAsync(createTagDto, userId);

    _logger.LogInformation("Created tag {TagId} with name '{Name}' for user {UserId}",
      createdTag.Id, createdTag.Name, userId);

    return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, createdTag);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateTag([FromRoute] Guid id, [FromBody] UpdateTagDto updateTagDto)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    var updatedTag = await _tagService.UpdateAsync(id, updateTagDto, userId);
    return Ok(updatedTag);
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteTag([FromRoute] Guid id)
  {
    var userId = GetCurrentUserId();
    if (userId == null) return Unauthorized();

    await _tagService.DeleteAsync(id, userId);
    return NoContent();
  }
}