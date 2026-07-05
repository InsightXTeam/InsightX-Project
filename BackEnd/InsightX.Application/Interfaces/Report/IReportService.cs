using InsightX.Application.DTOs.Reports;

namespace InsightX.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportResponseDto> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy);

        Task<List<ReportResponseDto>> GetReportsAsync(int companyId, string role, string userName);

        Task<string> GetStatusAsync(int id, int companyId);

        Task<string> GetPreviewAsync(int id, int companyId);

        Task<string> GetExtractedTextAsync(int id, int companyId);

        Task ConfirmTextAsync(int id, ConfirmReportDto dto, int companyId);

        Task DeleteAsync(int id, int companyId);
    }
}