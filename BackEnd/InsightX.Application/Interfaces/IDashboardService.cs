using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<List<KpiCardDto>> GetKpiCardsAsync();
        Task<List<TrendChartDto>> GetTrendsAsync(string kpiName);
        Task<List<DepartmentSummaryDto>> GetDepartmentSummariesAsync();
        Task<List<AlertDto>> GetRecentAlertsAsync();
    }
}
