using Microsoft.AspNetCore.Http;

namespace InsightX.Application.DTOs.Reports;

public class UploadReportDto
{
    public string ReportName { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}