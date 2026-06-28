using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs.Auth;

namespace InsightX.Application.Interfaces.Auth
{
    public interface ICompanyService
    {
        Task<ServiceResult<CompanyProfileDto>> GetMyCompanyAsync(int companyId);
        Task<ServiceResult> SetupAsync(SetupDto dto, int companyId);
    }
}
