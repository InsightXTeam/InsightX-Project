using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs.Auth;

namespace InsightX.Application.Interfaces.Auth
{
    public interface IKpiService
    {
        Task<ServiceResult<KpiResponseDto>> CreateAsync(CreateKpiDto dto, int companyId);
        Task<ServiceResult<KpiResponseDto>> GetByIdAsync(int id, int companyId);
        Task<ServiceResult<List<KpiResponseDto>>> GetAllAsync(int companyId);
        Task<ServiceResult<KpiResponseDto>> UpdateAsync(int id, CreateKpiDto dto, int companyId);
        Task<ServiceResult> DeleteAsync(int id, int companyId);
    }
}
