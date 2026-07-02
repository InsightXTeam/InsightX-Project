namespace InsightX.Application.Interfaces
{
    public interface IMetricsRepository
    {
        Task<decimal?> GetThresholdAsync(int companyId, string kpiName);
        Task<decimal?> GetSameMonthLastYearAsync(int companyId, string kpiName, int month);
        Task<List<decimal>> GetLastNMonthsAsync(int companyId, string kpiName, int n);
    }
}
