using Microsoft.EntityFrameworkCore;
using ProjectManagement.Infrastructure.Data;
using ProjectManagement.Domain;
using ProjectManagement.Application.Interfaces.Repositories;

namespace ProjectManagement.Infrastructure.Repositories;

public class ColumnRepository : IColumnRepository
{
  private readonly AppDbContext _context;

  public ColumnRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<Column>> GetAllByWorkspaceAsync(Guid workspaceId)
  {
    return await _context.Columns
      .Where(c => c.WorkspaceId == workspaceId)
      .OrderBy(c => c.Position)
      .ToListAsync();
  }

  public async Task<Column?> GetByIdAsync(Guid id)
  {
    return await _context.Columns
      .Include(c => c.Workspace)
      .FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task<Column> CreateAsync(Column column)
  {
    _context.Columns.Add(column);
    await _context.SaveChangesAsync();

    return column;
  }

  public async Task<Column> UpdateAsync(Column column)
  {
    _context.Columns.Update(column);
    await _context.SaveChangesAsync();

    return column;
  }

  public async Task DeleteAsync(Column column)
  {
    _context.Columns.Remove(column);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> WorkspaceExistsAsync(Guid workspaceId)
  {
    return await _context.Workspaces.AnyAsync(w => w.Id == workspaceId);
  }

  public async Task<bool> UserHasAccessToWorkspaceAsync(Guid workspaceId, string userId)
  {
    return await _context.Workspaces
      .AnyAsync(w => w.Id == workspaceId && w.UserId == userId);
  }

  public async Task<bool> UserHasAccessToColumnAsync(Guid columnId, string userId)
  {
    return await _context.Columns
      .Include(c => c.Workspace)
      .AnyAsync(c => c.Id == columnId && c.Workspace!.UserId == userId);
  }

  public async Task<int> GetMaxPositionAsync(Guid workspaceId)
  {
    var maxPosition = await _context.Columns
      .Where(c => c.WorkspaceId == workspaceId)
      .Select(c => (int?)c.Position)
      .MaxAsync();

    return maxPosition ?? -1;
  }

  public async Task<bool> ColumnExistsInWorkspaceAsync(Guid columnId, Guid workspaceId)
  {
    return await _context.Columns
      .AnyAsync(c => c.Id == columnId && c.WorkspaceId == workspaceId);
  }
}