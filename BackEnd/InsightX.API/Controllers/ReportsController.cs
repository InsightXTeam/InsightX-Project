using System.Security.Claims;
using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        // POST   => /reports/upload
        // GET    => /reports
        // GET    => /reports/{id}/status
        // GET    => /reports/{id}/preview
        // GET    => /reports/{id}/text
        // POST   => /reports/{id}/confirm
        // DELETE => /reports/{id}
        // POST   => /reports/{id}/process

        private readonly IReportService _service;

        private readonly IDocumentProcessor _processor;

        public ReportsController(IReportService service, IDocumentProcessor processor)
        {
            _service = service;

            _processor = processor;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadReportDto dto)
        {
            var companyIdClaim = User.FindFirstValue("CompanyId");
            var departmentIdClaim = User.FindFirstValue("DepartmentId");
            var userName = User.Identity?.Name ?? "Unknown";

            if (!int.TryParse(companyIdClaim, out int companyId))
                return Unauthorized("Company ID is missing from token.");

            int? departmentId = null;
            if (int.TryParse(departmentIdClaim, out int parsedDeptId))
                departmentId = parsedDeptId;

            var result = await _service.UploadAsync(dto, companyId, departmentId, userName);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetReportsAsync();
            return Ok(result);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Status(int id)
        {
            var result = await _service.GetStatusAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> Preview(int id)
        {
            var result = await _service.GetPreviewAsync(id);
            return Content(result, "application/json");
        }

        [HttpGet("{id}/text")]
        public async Task<IActionResult> GetText(int id)
        {
            var result = await _service.GetExtractedTextAsync(id);
            return Ok(new { text = result });
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            await _service.ConfirmAsync(id);
            return Ok(new { message = "Data confirmed successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> Process(int id)
        {
            await _processor.ProcessAsync(id);

            return Ok("Processing finished");
        }
    }
}