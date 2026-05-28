using Microsoft.EntityFrameworkCore;
using TrackingService.Models;

namespace TrackingService.Data;

public class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options) : base(options) { }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Score> Scores { get; set; }
}