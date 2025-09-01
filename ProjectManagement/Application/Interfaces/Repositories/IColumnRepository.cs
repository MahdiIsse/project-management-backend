using ProjectManagement.Domain;

namespace ProjectManagement.Application.Interfaces.Repositories;
public interface IColumnRepository
{
  Task<IEnumerable<Column>> GetAllByWorkspaceAsync(Guid workspaceId);
  Task<Column?> GetByIdAsync(Guid id);
  Task<Column> CreateAsync(Column column);
  Task<Column> UpdateAsync(Column column);
  Task DeleteAsync(Column column);
  Task<bool> WorkspaceExistsAsync(Guid workspaceId);
  Task<bool> UserHasAccessToWorkspaceAsync(Guid workspaceId, string userId);
  Task<bool> UserHasAccessToColumnAsync(Guid columnId, string userId);
  Task<bool> ColumnExistsInWorkspaceAsync(Guid columnId, Guid workspaceId);
  Task<int> GetMaxPositionAsync(Guid workspaceId);
}