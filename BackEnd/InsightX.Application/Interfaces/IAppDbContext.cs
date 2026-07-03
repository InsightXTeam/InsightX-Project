using System.Threading;
using System.Threading.Tasks;
using InsightX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightX.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Company> Companies { get; }
        DbSet<Department> Departments { get; }
        DbSet<User> Users { get; }
        DbSet<Report> Reports { get; }
        DbSet<ExtractedMetric> ExtractedMetrics { get; }
        DbSet<KpiThreshold> KpiThresholds { get; }
        DbSet<Alert> Alerts { get; }
        DbSet<Conversation> Conversations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
