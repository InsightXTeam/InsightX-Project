using InsightX.Domain.ValueObjects;

namespace InsightX.Application.Interfaces
{
    public interface IAnomalyDetector
    {
        Task<AnomalyResult> CheckAsync(
            int companyId,
            int departmentId,
            string kpiName,
            decimal currentValue);
    }
}
