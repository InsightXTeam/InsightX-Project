using Asp.Versioning;
using InsightX.Application.DTOs;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/alerts")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IGetAlertsUseCase _getAlerts;
        private readonly IMarkAlertSeenUseCase _markSeen;
        private readonly IGenerateAlertUseCase _generateAlert;

        public AlertsController(
            IGetAlertsUseCase getAlerts,
            IMarkAlertSeenUseCase markSeen,
            IGenerateAlertUseCase generateAlert)
        {
            _getAlerts = getAlerts;
            _markSeen = markSeen;
            _generateAlert = generateAlert;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts([FromQuery] bool? seen, CancellationToken cancellationToken)
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                return Forbid("Invalid or missing companyId claim.");
            }

            var alerts = await _getAlerts.ExecuteAsync(companyId, seen, cancellationToken);
            return Ok(alerts);
        }

        [HttpPut("{id}/seen")]
        public async Task<IActionResult> MarkSeen(int id, CancellationToken cancellationToken)
        {
            await _markSeen.ExecuteAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunDetection([FromBody] RunAlertRequest request, CancellationToken cancellationToken)
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                return Forbid("Invalid or missing companyId claim.");
            }

            await _generateAlert.ExecuteAsync(
                companyId,
                request.DepartmentId,
                request.KpiName,
                request.CurrentValue,
                cancellationToken);

            return Ok(new { message = "Anomaly detection triggered successfully." });
        }
    }
}
