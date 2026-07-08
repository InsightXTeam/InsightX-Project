using Microsoft.AspNetCore.Http;

namespace InsightX.Application.DTOs.Reports;

public class UploadReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
    public int ReportMonth { get; set; }
    public int ReportYear { get; set; }
    public int? DepartmentId { get; set; }
}