using System.Collections.Generic;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<ServiceResult<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, int companyId);
        Task<ServiceResult<DepartmentResponseDto>> GetByIdAsync(int id, int companyId);
        Task<ServiceResult<List<DepartmentResponseDto>>> GetAllAsync(int companyId);
    }
}
