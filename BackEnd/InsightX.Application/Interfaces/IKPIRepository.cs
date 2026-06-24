using InsightX.Domain.Entities;

namespace InsightX.Application.Interfaces
{
    public interface IKPIRepository
    {
        Task<List<string>> GetKpiNamesByCompanyIdAsync(int companyId);
    }
}
