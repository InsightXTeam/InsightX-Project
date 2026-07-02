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

        public AlertsController(GetAlertsUseCase getAlerts, MarkAlertSeenUseCase markSeen)
        {
            _getAlerts = getAlerts;
            _markSeen = markSeen;
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
    }
}
