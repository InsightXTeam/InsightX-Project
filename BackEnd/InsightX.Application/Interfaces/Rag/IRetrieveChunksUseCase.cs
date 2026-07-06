using InsightX.Application.Common;
using InsightXAI.Application.DTOs;

namespace InsightXAI.Application.Interfaces.Rag
{
    public interface IRetrieveChunksUseCase
    {
        Task<ServiceResult<RetrieveResponseDto>> ExecuteAsync(RetrieveRequestDto request, int companyId, int? departmentId, CancellationToken cancellationToken = default);
    }
}
