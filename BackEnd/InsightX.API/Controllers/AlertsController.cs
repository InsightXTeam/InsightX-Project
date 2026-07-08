using Asp.Versioning;
using InsightX.Application.DTOs;
using InsightX.Application.Extensions;
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
        private readonly IMarkAllAlertsSeenUseCase _markAllSeen;
        private readonly IGenerateAlertUseCase _generateAlert;
        private readonly IDeleteAlertUseCase _deleteAlert;

        public AlertsController(
            IGetAlertsUseCase getAlerts,
            IMarkAlertSeenUseCase markSeen,
            IMarkAllAlertsSeenUseCase markAllSeen,
            IGenerateAlertUseCase generateAlert,
            IDeleteAlertUseCase deleteAlert)
        {
            _getAlerts = getAlerts;
            _markSeen = markSeen;
            _markAllSeen = markAllSeen;
            _generateAlert = generateAlert;
            _deleteAlert = deleteAlert;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts([FromQuery] bool? seen, CancellationToken cancellationToken)
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                return Forbid("Invalid or missing companyId claim.");
            }

            int? departmentId = null;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role == "Manager")
            {
                departmentId = User.GetDepartmentId();
            }

            var alerts = await _getAlerts.ExecuteAsync(companyId, seen, departmentId, cancellationToken);
            return Ok(alerts);
        }

        [HttpPut("{id}/seen")]
        public async Task<IActionResult> MarkSeen(int id, CancellationToken cancellationToken)
        {
            await _markSeen.ExecuteAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPut("seen-all")]
        public async Task<IActionResult> MarkAllSeen(CancellationToken cancellationToken)
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                return Forbid("Invalid or missing companyId claim.");
            }

            int? departmentId = null;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role == "Manager")
            {
                departmentId = User.GetDepartmentId();
            }

            await _markAllSeen.ExecuteAsync(companyId, departmentId, cancellationToken);
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
                null,
                cancellationToken);

            return Ok(new { message = "Anomaly detection triggered successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id, CancellationToken cancellationToken)
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                return Forbid("Invalid or missing companyId claim.");
            }

            int? departmentId = null;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            if (role == "Manager")
            {
                departmentId = User.GetDepartmentId();
            }

            await _deleteAlert.ExecuteAsync(id, companyId, departmentId, role, cancellationToken);
            return NoContent();
        }
    }
}
