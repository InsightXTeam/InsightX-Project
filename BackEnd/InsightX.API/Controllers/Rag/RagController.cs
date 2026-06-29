using InsightXAI.Application.DTOs;
using InsightXAI.Application.UseCases.Rag;
using Microsoft.AspNetCore.Mvc;

namespace InsightXAI.API.Controllers
{
    /// <summary>
    /// Endpoints for report indexing and vector retrieval operations.
    /// </summary>
    [ApiController]
    [Route("rag")]
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
            var result = await _indexReportUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(result);
        }

        // Retrieves relevant chunks for a question.
        [HttpPost("retrieve")]
        public async Task<ActionResult<RetrieveResponseDto>> RetrieveChunks(
            [FromBody] RetrieveRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _retrieveChunksUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(result);
        }

        // Deletes all indexed chunks for a report.
        [HttpDelete("{companyId:int}/{reportId:int}")]
        public async Task<IActionResult> DeleteReportChunks(int companyId, int reportId, CancellationToken cancellationToken)
        {
            await _deleteReportChunksUseCase.ExecuteAsync(companyId, reportId, cancellationToken);
            return NoContent();
        }
    }
}
