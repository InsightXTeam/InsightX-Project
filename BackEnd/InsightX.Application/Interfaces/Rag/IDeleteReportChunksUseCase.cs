using InsightX.Application.Common;

namespace InsightXAI.Application.Interfaces.Rag
{
    public interface IDeleteReportChunksUseCase
    {
        Task<ServiceResult> ExecuteAsync(int companyId, int reportId, string userId, string role, CancellationToken cancellationToken = default);
    }
}
