using ProjectManagement.Application.DTOs.Workspaces;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IWorkspaceService
{
  Task<IEnumerable<WorkspaceDto>> GetAllAsync(string userId);
  Task<WorkspaceDto> GetByIdAsync(Guid id, string userId);
  Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto createWorkspaceDto, string userId);
  Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto updateWorkspaceDto, string userId);
  Task DeleteAsync(Guid id, string userId);
}