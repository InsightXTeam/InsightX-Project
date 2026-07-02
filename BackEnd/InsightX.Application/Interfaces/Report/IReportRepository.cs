using InsightX.Domain.Entities.Reports;

namespace InsightX.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<List<Report>> GetAllAsync();

        Task<Report?> GetByIdAsync(int id);

        Task<List<Report>> GetByCompanyAndUserAsync(int companyId, string role, string userName);

        Task AddAsync(Report report);

        Task UpdateAsync(Report report);

        Task DeleteAsync(Report report);
    }
}