using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Domain;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Repositories;

public class AssigneeRepository : IAssigneeRepository
{
  private readonly AppDbContext _context;
  public AssigneeRepository(AppDbContext context)
  {
    _context = context;
  }
  public async Task<IEnumerable<Assignee>> GetAllByUserIdAsync(string userId)
  {
    return await _context.Assignees.Where(a => a.UserId == userId).ToListAsync();
  }

  public async Task<Assignee?> GetByIdAsync(Guid id)
  {
    return await _context.Assignees.FindAsync(id);
  }

  public async Task<Assignee> CreateAsync(Assignee assignee)
  {
    await _context.Assignees.AddAsync(assignee);
    await _context.SaveChangesAsync();

    return assignee;
  }

  public async Task<Assignee> UpdateAsync(Assignee assignee)
  {
    _context.Assignees.Update(assignee);
    await _context.SaveChangesAsync();

    return assignee;
  }

  public async Task DeleteAsync(Assignee assignee)
  {
    _context.Assignees.Remove(assignee);
    await _context.SaveChangesAsync();
  }
}