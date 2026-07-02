using InsightX.Application.DTOs;
using InsightX.Application.UseCases.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly GetAlertsUseCase _getAlerts;
        private readonly MarkAlertSeenUseCase _markSeen;
        private readonly GenerateAlertUseCase _generateAlert;

        public AlertsController(
            GetAlertsUseCase getAlerts,
            MarkAlertSeenUseCase markSeen,
            GenerateAlertUseCase generateAlert)
        {
            _getAlerts = getAlerts;
            _markSeen = markSeen;
            _generateAlert = generateAlert;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts([FromQuery] bool? seen)
        {
            var companyId = int.Parse(User.FindFirst("companyId")!.Value);
            var alerts = await _getAlerts.ExecuteAsync(companyId, seen);
            return Ok(alerts);
        }

        [HttpPut("{id}/seen")]
        public async Task<IActionResult> MarkSeen(int id)
        {
            await _markSeen.ExecuteAsync(id);
            return NoContent();
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunDetection([FromBody] RunAlertRequest request)
        {
            var companyId = int.Parse(User.FindFirst("companyId")!.Value);

            await _generateAlert.ExecuteAsync(
                companyId,
                request.DepartmentId,
                request.KpiName,
                request.CurrentValue);

            return Ok(new { message = "Anomaly detection triggered successfully." });
        }
    }
}
