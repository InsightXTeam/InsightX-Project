using InsightX.Domain.Entities.Reports;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Report> Reports { get; set; }

        public DbSet<ExtractedMetric> ExtractedMetrics { get; set; }
    }
}