using InsightX.Application.Extensions;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.Interfaces.Rag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace InsightXAI.API.Controllers.Rag
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class RagController : ControllerBase
    {
        private readonly IIndexReportUseCase _indexReportUseCase;
        private readonly IRetrieveChunksUseCase _retrieveChunksUseCase;
        private readonly IDeleteReportChunksUseCase _deleteReportChunksUseCase;

        public RagController(
            IIndexReportUseCase indexReportUseCase,
            IRetrieveChunksUseCase retrieveChunksUseCase,
            IDeleteReportChunksUseCase deleteReportChunksUseCase)
        {
            _indexReportUseCase = indexReportUseCase;
            _retrieveChunksUseCase = retrieveChunksUseCase;
            _deleteReportChunksUseCase = deleteReportChunksUseCase;
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexReport(
            [FromBody] IndexReportRequestDto request,
            CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var departmentId = User.GetDepartmentId();
            var userId = User.GetUserId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _indexReportUseCase.ExecuteAsync(request, companyId, departmentId, userId, role, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpPost("retrieve")]
        public async Task<IActionResult> RetrieveChunks(
            [FromBody] RetrieveRequestDto request,
            CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var departmentId = User.GetDepartmentId(); // From develop merge, returns int?

            var result = await _retrieveChunksUseCase.ExecuteAsync(request, companyId, departmentId, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Data);
        }

        [HttpDelete("{reportId:int}")]
        public async Task<IActionResult> DeleteReportChunks(int reportId, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var userId = User.GetUserId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

            var result = await _deleteReportChunksUseCase.ExecuteAsync(companyId, reportId, userId, role, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return NoContent();
        }
    }
}
