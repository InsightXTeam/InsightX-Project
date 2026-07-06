using InsightX.Application.Common;
using System.Threading;
using System.Threading.Tasks;

namespace InsightX.Application.Interfaces
{
    public interface IDocumentProcessor
    {
        Task<ServiceResult> ProcessAsync(int reportId, int companyId, string role, string userName, CancellationToken cancellationToken = default);

        Task<ServiceResult> ExtractKpisAsync(int reportId, int companyId, string role, string userName, CancellationToken cancellationToken = default);
    }
}