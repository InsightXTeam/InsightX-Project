using System.Threading.Tasks;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Linq;
using Asp.Versioning;

namespace InsightX.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private int GetCompanyId()
        {
            return int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
        }

        private int? GetDepartmentId()
        {
            var deptIdStr = User.FindFirst("DepartmentId")?.Value;
            if (string.IsNullOrEmpty(deptIdStr)) return null;
            return int.Parse(deptIdStr);
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var result = await _dashboardService.GetKpisSummaryAsync(companyId, GetDepartmentId());
            return Ok(result);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends([FromQuery] int months = 6)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var result = await _dashboardService.GetTrendsAsync(companyId, GetDepartmentId(), months);
            return Ok(result);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            // Typically department performance is for owners, but if a manager asks, maybe just return theirs
            // For now, let's just use company ID. A manager could just see their own.
            var result = await _dashboardService.GetDepartmentsPerformanceAsync(companyId);

            var deptId = GetDepartmentId();
            if (deptId.HasValue)
            {
                result = result.Where(d => d.DepartmentId == deptId.Value).ToList();
            }

            return Ok(result);
        }

        [HttpGet("alerts/recent")]
        public async Task<IActionResult> GetRecentAlerts()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var result = await _dashboardService.GetRecentAlertsAsync(companyId, GetDepartmentId());
            return Ok(result);
        }
    }
}
