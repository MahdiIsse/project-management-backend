using ProjectManagement.Domain;

namespace ProjectManagement.Application.Interfaces.Repositories;

public interface IProjectTaskRepository
{
  Task<IEnumerable<ProjectTask>> GetAllByWorkspaceIdAsync(Guid workspaceId);
  Task<ProjectTask?> GetByIdAsync(Guid taskId);
  Task<ProjectTask> CreateAsync(ProjectTask projectTask);
  Task<ProjectTask> UpdateAsync(ProjectTask projectTask);
  Task DeleteAsync(ProjectTask task);
  Task<int> GetMaxPositionAsync(Guid columnId);
  Task<ProjectTask> AddAssigneeToTaskAsync(ProjectTask projectTask, Assignee assignee);
  Task<ProjectTask> RemoveAssigneeFromTaskAsync(ProjectTask projectTask, Assignee assignee);
  Task<ProjectTask> AddTagToTaskAsync(ProjectTask projectTask, Tag tag);
  Task<ProjectTask> RemoveTagFromTaskAsync(ProjectTask projectTask, Tag tag);
}