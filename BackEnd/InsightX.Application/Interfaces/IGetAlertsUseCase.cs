using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IGetAlertsUseCase
    {
        Task<List<AlertDto>> ExecuteAsync(int companyId, bool? seenFilter, CancellationToken cancellationToken = default);
    }
}
