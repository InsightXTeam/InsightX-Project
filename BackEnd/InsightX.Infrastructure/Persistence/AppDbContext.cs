using InsightX.Domain.Entities.Reports;
using InsightX.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Report> Reports { get; set; }

        public DbSet<ExtractedMetric> ExtractedMetrics { get; set; }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<KPI> KPIs => Set<KPI>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Required for Identity tables

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Department>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<KPI>()
                .HasOne(k => k.Company)
                .WithMany(c => c.KPIs)
                .HasForeignKey(k => k.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}