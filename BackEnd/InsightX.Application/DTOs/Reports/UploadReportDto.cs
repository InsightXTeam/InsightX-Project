using Microsoft.AspNetCore.Http;

namespace InsightX.Application.DTOs.Reports;

public class UploadReportDto
{
    public IFormFile File { get; set; } = null!;
    //public IFormFile? File { get; set; }
}