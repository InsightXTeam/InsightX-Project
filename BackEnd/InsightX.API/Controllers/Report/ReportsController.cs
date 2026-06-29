using InsightX.Application.DTOs.Reports;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsightX.API.Controllers.Report
{
    // [Authorize] // Temporarily disabled for isolated testing
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
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] // 100MB
        public async Task<IActionResult> Upload([FromForm] UploadReportDto dto)
        {
            // Temporarily mocked for isolated testing without Auth token
            var companyIdClaim = "1"; // Mocked
            var departmentIdClaim = "1"; // Mocked
            var userName = "Test User";

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
            // Temporarily mocked for isolated testing without Auth token
            var companyIdClaim = "1"; // Mocked
            var roleClaim = "Admin"; // Mocked
            var userName = "Test User";

            if (!int.TryParse(companyIdClaim, out int companyId))
                return Unauthorized("Company ID is missing from token.");

            var result = await _service.GetReportsAsync(companyId, roleClaim ?? "", userName);
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
        public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmReportDto dto)
        {
            await _service.ConfirmTextAsync(id, dto);
            await _processor.ExtractKpisAsync(id);
            return Ok(new { message = "Text confirmed and KPIs extracted successfully" });
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

            return Ok(new { message = "Processing finished" });
        }
    }
}