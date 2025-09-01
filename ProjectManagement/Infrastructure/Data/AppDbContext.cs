using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Domain;

namespace ProjectManagement.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<Workspace> Workspaces { get; set; }
  public DbSet<Column> Columns { get; set; }
  public DbSet<ProjectTask> Tasks { get; set; }
  public DbSet<Tag> Tags { get; set; }
  public DbSet<Assignee> Assignees { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<Column>()
      .HasOne(c => c.Workspace)
      .WithMany()
      .HasForeignKey(c => c.WorkspaceId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<ProjectTask>()
      .HasOne(t => t.Column)
      .WithMany()
      .HasForeignKey(t => t.ColumnId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<ProjectTask>()
      .HasOne(t => t.Workspace)
      .WithMany()
      .HasForeignKey(t => t.WorkspaceId)
      .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<ProjectTask>()
      .HasMany(t => t.Assignees)
      .WithMany(a => a.Tasks);

    builder.Entity<ProjectTask>()
      .HasMany(t => t.Tags)
      .WithMany(tags => tags.Tasks);

    var adminRoleId = "f6bde1b5-8a16-4a64-9d1e-4f61bdc2a001";
    var userRoleId = "a1f3b9d0-6c2b-4f2d-8a2c-11c4b2d9a002";

    builder.Entity<IdentityRole>().HasData(
    new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = adminRoleId },
    new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER", ConcurrencyStamp = userRoleId }
    );
  }
}