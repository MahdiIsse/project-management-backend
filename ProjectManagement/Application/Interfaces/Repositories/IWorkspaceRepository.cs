using ProjectManagement.Domain;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IWorkspaceRepository
{
  Task<IEnumerable<Workspace>> GetAllByUserIdAsync(string userId);
  Task<Workspace?> GetByIdAsync(Guid id);
  Task<Workspace> CreateAsync(Workspace workspace);
  Task<Workspace> UpdateAsync(Workspace workspace);
  Task DeleteAsync(Workspace workspace);
  Task<int> GetMaxPositionAsync(string userId);
  Task<bool> UserHasAccessToWorkspaceAsync(Guid workspaceId, string userId);
}