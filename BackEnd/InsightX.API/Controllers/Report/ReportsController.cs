using Asp.Versioning;
using InsightX.Application.DTOs.Reports;
using InsightX.Application.Extensions;
using InsightX.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace InsightX.API.Controllers.Report
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        public async Task<IActionResult> Upload([FromForm] UploadReportDto dto, CancellationToken cancellationToken)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded or the file is empty." });
            }

            var companyId = User.GetCompanyId();
            var departmentId = User.GetDepartmentId();
            var userName = User.GetUserId();

            var result = await _service.UploadAsync(dto, companyId, departmentId, userName, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var userName = User.GetUserId();

            var result = await _service.GetReportsAsync(companyId, role, userName, cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Status(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetStatusAsync(id, User.GetCompanyId(), cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("{id}/preview")]
        public async Task<IActionResult> Preview(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetPreviewAsync(id, User.GetCompanyId(), cancellationToken);
            return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpGet("{id}/text")]
        public async Task<IActionResult> GetText(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetExtractedTextAsync(id, User.GetCompanyId(), cancellationToken);
            return result.IsSuccess ? Ok(new { text = result.Data }) : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmReportDto dto, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var userName = User.GetUserId();

            var confirmResult = await _service.ConfirmTextAsync(id, dto, companyId, role, userName, cancellationToken);
            if (!confirmResult.IsSuccess) return StatusCode(confirmResult.StatusCode, confirmResult.Error);

            return Ok(new { message = "KPIs confirmed successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var result = await _service.DeleteAsync(id, User.GetCompanyId(), role, User.GetUserId(), cancellationToken);
            return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
        }

        [HttpPost("{id}/process")]
        public async Task<IActionResult> Process(int id, CancellationToken cancellationToken)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var result = await _processor.ProcessAsync(id, User.GetCompanyId(), role, User.GetUserId(), cancellationToken);
            return result.IsSuccess ? Ok(new { message = "Processing finished" }) : StatusCode(result.StatusCode, result.Error);
        }
    }
}