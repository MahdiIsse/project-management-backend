using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Domain;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Repositories;

public class ProjectTaskRepository : IProjectTaskRepository
{
  private readonly AppDbContext _context;

  public ProjectTaskRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<ProjectTask>> GetAllByWorkspaceIdAsync(Guid workspaceId)
  {
    return await _context.Tasks
      .Include(t => t.Assignees)
      .Include(t => t.Tags)
      .Where(t => t.WorkspaceId == workspaceId)
      .OrderBy(t => t.Position)
      .ToListAsync();
  }

  public async Task<ProjectTask?> GetByIdAsync(Guid taskId)
  {
    return await _context.Tasks
    .Include(t => t.Assignees)
    .Include(t => t.Tags)
    .FirstOrDefaultAsync(t => t.Id == taskId);
  }

  public async Task<ProjectTask> CreateAsync(ProjectTask projectTask)
  {
    await _context.Tasks.AddAsync(projectTask);
    await _context.SaveChangesAsync();

    return projectTask;
  }

  public async Task<ProjectTask> UpdateAsync(ProjectTask projectTask)
  {
    _context.Tasks.Update(projectTask);
    await _context.SaveChangesAsync();

    return projectTask;
  }

  public async Task DeleteAsync(ProjectTask projectTask)
  {
    _context.Tasks.Remove(projectTask);
    await _context.SaveChangesAsync();
  }

  public async Task<int> GetMaxPositionAsync(Guid columnId)
  {
    var maxPosition = await _context.Tasks
        .Where(t => t.ColumnId == columnId)
        .Select(t => (int?)t.Position)
        .MaxAsync();

    return maxPosition ?? -1;
  }

  public async Task<ProjectTask> AddAssigneeToTaskAsync(ProjectTask task, Assignee assignee)
  {
    task.Assignees.Add(assignee);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task<ProjectTask> RemoveAssigneeFromTaskAsync(ProjectTask task, Assignee assignee)
  {
    task.Assignees.Remove(assignee);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task<ProjectTask> AddTagToTaskAsync(ProjectTask task, Tag tag)
  {
    task.Tags.Add(tag);
    await _context.SaveChangesAsync();
    return task;
  }

  public async Task<ProjectTask> RemoveTagFromTaskAsync(ProjectTask task, Tag tag)
  {
    task.Tags.Remove(tag);
    await _context.SaveChangesAsync();
    return task;
  }
}