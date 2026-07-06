using InsightX.Domain.Entities.Reports;

namespace InsightX.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<List<Report>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<Report?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Report?> GetByIdForCompanyAsync(int id, int companyId, CancellationToken cancellationToken = default);

        Task<List<Report>> GetByCompanyAndUserAsync(int companyId, string role, string userName, CancellationToken cancellationToken = default);

        Task AddAsync(Report report, CancellationToken cancellationToken = default);

        Task UpdateAsync(Report report, CancellationToken cancellationToken = default);

        Task DeleteAsync(Report report, CancellationToken cancellationToken = default);
    }
}