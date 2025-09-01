using ProjectManagement.Application.DTOs.ProjectTasks;

namespace ProjectManagement.Application.Interfaces.Services;

public interface IProjectTaskService
{
  Task<IEnumerable<ProjectTaskDto>> GetAllByWorkspaceIdAsync(Guid workspaceId, string userId);
  Task<ProjectTaskDto> GetByIdAsync(Guid taskId, string userId);
  Task<ProjectTaskDto> CreateAsync(Guid workspaceId, Guid columnId, CreateProjectTaskDto createProjectTaskDto, string userId);
  Task<ProjectTaskDto> UpdateAsync(Guid taskId, UpdateProjectTaskDto updateProjectTaskDto, string userId);
  Task DeleteAsync(Guid taskId, string userId);
  Task<ProjectTaskDto> AddAssigneeToTaskAsync(Guid taskId, Guid assigneeId, string userId);
  Task<ProjectTaskDto> RemoveAssigneeFromTaskAsync(Guid taskId, Guid assigneeId, string userId);
  Task<ProjectTaskDto> AddTagToTaskAsync(Guid taskId, Guid tagId, string userId);
  Task<ProjectTaskDto> RemoveTagFromTaskAsync(Guid taskId, Guid tagId, string userId);
}