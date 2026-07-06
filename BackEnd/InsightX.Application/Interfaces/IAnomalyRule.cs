using InsightX.Application.DTOs;
using InsightX.Domain.ValueObjects;

namespace InsightX.Application.Interfaces
{
    public interface IAnomalyRule
    {
        Task<AnomalyResult> CheckAsync(int companyId, string kpiName, decimal currentValue, KPIConfig config);
    }
}
