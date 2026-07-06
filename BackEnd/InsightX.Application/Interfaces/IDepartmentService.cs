using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<ServiceResult<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<DepartmentResponseDto>> GetByIdAsync(int id, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<DepartmentResponseDto>>> GetAllAsync(int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult<DepartmentResponseDto>> UpdateAsync(int id, UpdateDepartmentDto dto, int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult> DeleteAsync(int id, int companyId, CancellationToken cancellationToken = default);
    }
}
