using InsightX.Application.Common;
using InsightXAI.Application.DTOs;

namespace InsightXAI.Application.Interfaces.Rag
{
    public interface IIndexReportUseCase
    {
        Task<ServiceResult<IndexReportResponseDto>> ExecuteAsync(IndexReportRequestDto request, int companyId, int? departmentId, string userId, string role, CancellationToken cancellationToken = default);
    }
}
