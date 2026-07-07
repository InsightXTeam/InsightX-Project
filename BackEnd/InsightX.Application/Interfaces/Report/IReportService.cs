using InsightX.Application.Common;
using InsightX.Application.DTOs.Reports;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InsightX.Application.Interfaces
{
    public interface IReportService
    {
        Task<ServiceResult<ReportResponseDto>> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy, CancellationToken cancellationToken = default);
        Task<ServiceResult<List<ReportResponseDto>>> GetReportsAsync(int companyId, int? departmentId, string role, string userName, CancellationToken cancellationToken = default);
        Task<ServiceResult<string>> GetStatusAsync(int id, int companyId, CancellationToken cancellationToken = default);

        Task<ServiceResult<List<ExtractedMetricDto>>> GetPreviewAsync(int id, int companyId, CancellationToken cancellationToken = default);

        Task<ServiceResult<string>> GetExtractedTextAsync(int id, int companyId, CancellationToken cancellationToken = default);

        Task<ServiceResult> ConfirmTextAsync(int id, ConfirmReportDto dto, int companyId, string role, string userName, CancellationToken cancellationToken = default);

        Task<ServiceResult> DeleteAsync(int id, int companyId, string role, string userName, CancellationToken cancellationToken = default);

        Task<ServiceResult<ReportDownloadDto>> DownloadAsync(int id, int companyId, CancellationToken cancellationToken = default);
    }
}