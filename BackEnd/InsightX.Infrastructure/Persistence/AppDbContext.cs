using InsightX.Domain.Entities;
using InsightX.Domain.Entities.Reports;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<KPI> KPIs => Set<KPI>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<ExtractedMetric> ExtractedMetrics => Set<ExtractedMetric>();
        public DbSet<Alert> Alerts => Set<Alert>();

        public DbSet<Conversation> Conversations => Set<Conversation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Required for Identity tables

            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u => !u.IsDeleted);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithOne(d => d.User)
                .HasForeignKey<ApplicationUser>(u => u.DepartmentId)
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

            builder.Entity<KPI>()
                .HasOne(k => k.Department)
                .WithMany(d => d.KPIs)
                .HasForeignKey(k => k.DepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Report>()
                .HasOne(r => r.Company)
                .WithMany()
                .HasForeignKey(r => r.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Report>()
                .HasOne(r => r.UploadedBy)
                .WithMany()
                .HasForeignKey(r => r.UploadedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Report>()
                .HasOne(r => r.Department)
                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ExtractedMetric>()
                .HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Alert>(entity =>
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

                entity.HasOne(a => a.Report)
                      .WithMany()
                      .HasForeignKey(a => a.ReportId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.SessionId).IsRequired();
                entity.Property(c => c.Question).IsRequired();
                entity.Property(c => c.Answer).IsRequired();

                entity.HasOne(c => c.Company)
                      .WithMany()
                      .HasForeignKey(c => c.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
