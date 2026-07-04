using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<ServiceResult<CompanyProfileDto>> GetMyCompanyAsync(int companyId);
        Task<ServiceResult> SetupAsync(SetupDto dto, int companyId);
    }
}
