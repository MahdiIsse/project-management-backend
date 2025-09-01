using Microsoft.EntityFrameworkCore;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Domain;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
  private readonly AppDbContext _context;

  public TagRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<Tag>> GetAllByUserIdAsync(string userId)
  {
    return await _context.Tags.Where(t => t.UserId == userId).ToListAsync();
  }

  public async Task<Tag?> GetByIdAsync(Guid id)
  {
    return await _context.Tags.FindAsync(id);
  }

  public async Task<Tag> CreateAsync(Tag tag)
  {
    await _context.Tags.AddAsync(tag);
    await _context.SaveChangesAsync();

    return tag;
  }

  public async Task<Tag> UpdateAsync(Tag tag)
  {
    _context.Tags.Update(tag);
    await _context.SaveChangesAsync();

    return tag;
  }

  public async Task DeleteAsync(Tag tag)
  {
    _context.Tags.Remove(tag);
    await _context.SaveChangesAsync();
  }
}