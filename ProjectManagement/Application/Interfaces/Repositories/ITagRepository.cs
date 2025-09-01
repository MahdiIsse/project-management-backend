using ProjectManagement.Domain;

namespace ProjectManagement.Application.Interfaces.Repositories;
public interface ITagRepository
{
  Task<IEnumerable<Tag>> GetAllByUserIdAsync(string userId);
  Task<Tag?> GetByIdAsync(Guid id);
  Task<Tag> CreateAsync(Tag tag);
  Task<Tag> UpdateAsync(Tag tag);
  Task DeleteAsync(Tag tag);
}