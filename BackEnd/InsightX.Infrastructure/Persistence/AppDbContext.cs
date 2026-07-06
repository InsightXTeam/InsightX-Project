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

        // will be owned by Person 1
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<KPI> KPIs { get; set; }

        // will be owned by Person 2
        public DbSet<HistoricalMetric> HistoricalMetrics { get; set; }

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
                entity.Property(a => a.AlertType).HasConversion<int>();

                entity.HasOne(a => a.Company)
                      .WithMany()
                      .HasForeignKey(a => a.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Department)
                      .WithMany()
                      .HasForeignKey(a => a.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<KPI>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(k => k.Name).IsRequired().HasMaxLength(100);
                entity.Property(k => k.Threshold).HasPrecision(18, 2);
            });

            // HistoricalMetric Table Config (TEMP)
            modelBuilder.Entity<HistoricalMetric>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.KPIName).IsRequired().HasMaxLength(100);
                entity.Property(h => h.Value).HasPrecision(18, 2);
            });
        }
    }
}
