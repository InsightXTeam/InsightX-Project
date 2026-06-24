using InsightX.Application.DTOs.Reports;

namespace InsightX.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportResponseDto> UploadAsync(UploadReportDto dto, int companyId, int? departmentId, string uploadedBy);

        Task<List<ReportResponseDto>> GetReportsAsync();

        Task<string> GetStatusAsync(int id);

        Task<string> GetPreviewAsync(int id);

        Task<string> GetExtractedTextAsync(int id);

        Task ConfirmAsync(int id);

        Task DeleteAsync(int id);
    }
}