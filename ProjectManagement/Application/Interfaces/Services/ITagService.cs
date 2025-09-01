using ProjectManagement.Application.DTOs.Tags;

namespace ProjectManagement.Application.Interfaces.Services;

public interface ITagService
{
  Task<IEnumerable<TagDto>> GetAllByUserIdAsync(string userId);
  Task<TagDto> GetByIdAsync(Guid id, string userId);
  Task<TagDto> CreateAsync(CreateTagDto createTagDto, string userId);
  Task<TagDto> UpdateAsync(Guid id, UpdateTagDto updateTagDto, string userId);
  Task DeleteAsync(Guid id, string userId);
}