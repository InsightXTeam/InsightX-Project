using System.Threading;
using System.Threading.Tasks;
using InsightX.Application.Common;
using InsightX.Application.DTOs;

namespace InsightX.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<ServiceResult<CompanyProfileDto>> GetMyCompanyAsync(int companyId, CancellationToken cancellationToken = default);
        Task<ServiceResult> SetupAsync(SetupDto dto, int companyId, CancellationToken cancellationToken = default);
    }
}
