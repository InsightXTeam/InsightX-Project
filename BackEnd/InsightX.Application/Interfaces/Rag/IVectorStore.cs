using InsightXAI.Application.DTOs;

namespace InsightXAI.Application.Interfaces
{
    public interface IVectorStore
    {
        Task UpsertAsync(List<VectorRecordDto> records, CancellationToken cancellationToken = default);
        Task<List<RetrievedChunkDto>> SearchAsync(float[] queryVector, int companyId, int topK, int? departmentId = null, CancellationToken cancellationToken = default);
        Task DeleteByReportIdAsync(int companyId, int reportId, CancellationToken cancellationToken = default);
    }
}
