using InsightX.Domain.Entities;

namespace InsightX.Application.Interfaces
{
    public interface IAlertRepository
    {
        Task AddAsync(Alert alert);
        Task AddRangeAsync(IEnumerable<Alert> alerts, CancellationToken cancellationToken = default);
        Task<List<Alert>> GetByCompanyAsync(int companyId, bool? seenFilter, int? departmentId = null);
        Task<Alert?> GetByIdAsync(int id);
        Task MarkAsSeenAsync(int id, CancellationToken cancellationToken = default);
        Task MarkAllAsSeenAsync(int companyId, int? departmentId = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(Alert alert, CancellationToken cancellationToken = default);
        Task DeleteByReportIdAsync(int reportId, CancellationToken cancellationToken = default);
    }
}
