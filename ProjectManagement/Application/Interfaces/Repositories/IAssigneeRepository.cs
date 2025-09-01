using ProjectManagement.Domain;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IAssigneeRepository
{
  Task<IEnumerable<Assignee>> GetAllByUserIdAsync(string userId);
  Task<Assignee?> GetByIdAsync(Guid id);
  Task<Assignee> CreateAsync(Assignee assignee);
  Task<Assignee> UpdateAsync(Assignee assignee);
  Task DeleteAsync(Assignee assignee);
}