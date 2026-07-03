using System.Threading.Tasks;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            var result = await _dashboardService.GetKpiCardsAsync();
            return Ok(result);
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetTrends([FromQuery] string kpiName)
        {
            if (string.IsNullOrEmpty(kpiName))
            {
                return BadRequest("KPI Name is required.");
            }
            var result = await _dashboardService.GetTrendsAsync(kpiName);
            return Ok(result);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var result = await _dashboardService.GetDepartmentSummariesAsync();
            return Ok(result);
        }

        [HttpGet("alerts/recent")]
        public async Task<IActionResult> GetRecentAlerts()
        {
            var result = await _dashboardService.GetRecentAlertsAsync();
            return Ok(result);
        }
    }
}
