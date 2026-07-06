using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IKpiService
    {
        Task<ServiceResult<KpiResponseDto>> CreateAsync(CreateKpiDto dto, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<KpiResponseDto>> GetByIdAsync(int id, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<KpiResponseDto>>> GetAllAsync(int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<KpiResponseDto>> UpdateAsync(int id, UpdateKpiDto dto, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default);
    }
}
