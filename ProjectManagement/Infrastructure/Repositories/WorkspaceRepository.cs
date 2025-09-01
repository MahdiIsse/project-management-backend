using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Domain;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
  private readonly AppDbContext _context;

  public WorkspaceRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<Workspace>> GetAllByUserIdAsync(string userId)
  {
    return await _context.Workspaces
      .Where(w => w.UserId == userId)
      .OrderBy(w => w.Position)
      .ToListAsync();
  }

  public async Task<Workspace?> GetByIdAsync(Guid id)
  {
    return await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == id);
  }

  public async Task<Workspace> CreateAsync(Workspace workspace)
  {
    await _context.Workspaces.AddAsync(workspace);
    await _context.SaveChangesAsync();

    return workspace;
  }

  public async Task<Workspace> UpdateAsync(Workspace workspace)
  {
    _context.Workspaces.Update(workspace);
    await _context.SaveChangesAsync();
    return workspace;
  }

  public async Task DeleteAsync(Workspace workspace)
  {
    _context.Workspaces.Remove(workspace);
    await _context.SaveChangesAsync();
  }

  public async Task<int> GetMaxPositionAsync(string userId)
  {
    var maxPosition = await _context.Workspaces
      .Where(w => w.UserId == userId)
      .Select(w => (int?)w.Position)
      .MaxAsync();

    return maxPosition ?? -1;
  }

  public async Task<bool> UserHasAccessToWorkspaceAsync(Guid workspaceId, string userId)
  {
    return await _context.Workspaces
      .AnyAsync(w => w.Id == workspaceId && w.UserId == userId);
  }
}