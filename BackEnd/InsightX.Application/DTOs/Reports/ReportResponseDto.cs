namespace InsightX.Application.DTOs.Reports
{
    public class ReportResponseDto
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}