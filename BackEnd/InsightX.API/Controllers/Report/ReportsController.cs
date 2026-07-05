using InsightX.Application.DTOs.Reports;
using InsightX.Application.Extensions;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightX.API.Controllers.Report
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;
        private readonly IDocumentProcessor _processor;

        public ReportsController(IReportService service, IDocumentProcessor processor)
        {
            _service = service;
            _processor = processor;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(104857600)]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public async Task<IActionResult> Upload([FromForm] UploadReportDto dto)
        {
            var companyId = User.GetCompanyId();
            var departmentId = User.GetDepartmentId();
            var userName = User.GetUserId();

            var result = await _service.UploadAsync(dto, companyId, departmentId, userName);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyId = User.GetCompanyId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var userName = User.GetUserId();

            var result = await _service.GetReportsAsync(companyId, role, userName);
            return Ok(result);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Status(int id)
        {
            var result = await _service.GetStatusAsync(id, User.GetCompanyId());
            return Ok(result);
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> Preview(int id)
        {
            var result = await _service.GetPreviewAsync(id, User.GetCompanyId());
            return Content(result, "application/json");
        }

        [HttpGet("{id}/text")]
        public async Task<IActionResult> GetText(int id)
        {
            var result = await _service.GetExtractedTextAsync(id, User.GetCompanyId());
            return Ok(new { text = result });
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmReportDto dto)
        {
            var companyId = User.GetCompanyId();
            await _service.ConfirmTextAsync(id, dto, companyId);
            await _processor.ExtractKpisAsync(id, companyId);
            return Ok(new { message = "Text confirmed and KPIs extracted successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id, User.GetCompanyId());
            return NoContent();
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> Process(int id)
        {
            await _processor.ProcessAsync(id, User.GetCompanyId());
            return Ok(new { message = "Processing finished" });
        }
    }
}