using Insight_test.All.Dto;
using InsightXAI.Application.DTOs;

namespace InsightXAI.Application.Interfaces
{
    /// <summary>
    /// Defines vector storage and retrieval operations.
    /// </summary>
    public interface IVectorStore
    {
        // Inserts or updates vector records / Store (or update) a batch of chunks + their vectors + metadata.
        // Called by IndexReportUseCase after chunking + embedding a report.
        Task UpsertAsync(List<VectorRecordDto> records, CancellationToken cancellationToken = default);

        // Searches for the most relevant chunks.
        Task<List<RetrievedChunkDto>> SearchAsync(float[] queryVector, int companyId, int topK, CancellationToken cancellationToken = default);

        // Deletes all records associated with a report.
        Task DeleteByReportIdAsync(int companyId, int reportId, CancellationToken cancellationToken = default);
    }
}
