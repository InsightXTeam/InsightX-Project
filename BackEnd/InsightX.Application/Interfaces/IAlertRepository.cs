using InsightX.Domain.Entities;

namespace InsightX.Application.Interfaces
{
    public interface IAlertRepository
    {
        Task AddAsync(Alert alert);
        Task<List<Alert>> GetByCompanyAsync(int companyId, bool? seenFilter);
        Task<Alert?> GetByIdAsync(int id);
        Task MarkAsSeenAsync(int id);
    }
}
