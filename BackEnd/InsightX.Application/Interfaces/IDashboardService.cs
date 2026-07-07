using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<List<DashboardKpiDto>> GetKpisSummaryAsync(int companyId, int? departmentId);
        Task<List<DashboardTrendDto>> GetTrendsAsync(int companyId, int? departmentId);
        Task<List<DashboardDepartmentPerformanceDto>> GetDepartmentsPerformanceAsync(int companyId);
        Task<List<AlertDto>> GetRecentAlertsAsync(int companyId, int? departmentId);
    }
}
