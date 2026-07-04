using InsightX.Application.Extensions;
using InsightXAI.Application.DTOs;
using InsightXAI.Application.UseCases.Rag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsightXAI.API.Controllers.Rag
{
    /// <summary>
    /// Endpoints for report indexing and vector retrieval operations.
    /// </summary>
    [ApiController]
    [Route("rag")]
    [Authorize]
    public class RagController : ControllerBase
    {
        private readonly IndexReportUseCase _indexReportUseCase;
        private readonly RetrieveChunksUseCase _retrieveChunksUseCase;
        private readonly DeleteReportChunksUseCase _deleteReportChunksUseCase;

        public RagController(
            IndexReportUseCase indexReportUseCase,
            RetrieveChunksUseCase retrieveChunksUseCase,
            DeleteReportChunksUseCase deleteReportChunksUseCase)
        {
            _indexReportUseCase = indexReportUseCase;
            _retrieveChunksUseCase = retrieveChunksUseCase;
            _deleteReportChunksUseCase = deleteReportChunksUseCase;
        }

        //Indexes a report into the vector store.
        [HttpPost("index")]
        public async Task<ActionResult<IndexReportResponseDto>> IndexReport(
            [FromBody] IndexReportRequestDto request,
            CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();
            var departmentId = User.GetDepartmentId();

            var result = await _indexReportUseCase.ExecuteAsync(request,
                companyId,
                departmentId, cancellationToken);

            return Ok(result);
        }

        // Retrieves relevant chunks for a question.
        [HttpPost("retrieve")]
        public async Task<ActionResult<RetrieveResponseDto>> RetrieveChunks(
            [FromBody] RetrieveRequestDto request,
            CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();

            var result = await _retrieveChunksUseCase.ExecuteAsync(request, companyId, cancellationToken);
            return Ok(result);
        }

        // Deletes all indexed chunks for a report.
        [HttpDelete("{reportId:int}")]
        public async Task<IActionResult> DeleteReportChunks(int reportId, CancellationToken cancellationToken)
        {
            var companyId = User.GetCompanyId();

            await _deleteReportChunksUseCase.ExecuteAsync(companyId, reportId, cancellationToken);
            return NoContent();
        }
    }
}
