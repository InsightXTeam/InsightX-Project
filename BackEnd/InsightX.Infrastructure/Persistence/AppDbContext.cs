using InsightX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Alert> Alerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.KPIName).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Message).IsRequired().HasMaxLength(500);
                entity.Property(a => a.Recommendation).HasMaxLength(500);
                entity.Property(a => a.CurrentValue).HasPrecision(18, 2);
                entity.Property(a => a.Threshold).HasPrecision(18, 2);
            });
        }
    }
}
