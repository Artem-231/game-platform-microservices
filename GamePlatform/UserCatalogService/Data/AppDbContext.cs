using Microsoft.EntityFrameworkCore;
using UserCatalogService.Models;

namespace UserCatalogService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<LibraryRecord> LibraryRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Наш составной ключ
        modelBuilder.Entity<LibraryRecord>()
            .HasKey(lr => new { lr.UserId, lr.GameId });

        // Уникальный email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
    }
}