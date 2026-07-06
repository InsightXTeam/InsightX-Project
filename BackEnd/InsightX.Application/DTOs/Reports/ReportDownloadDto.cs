namespace InsightX.Application.DTOs.Reports
{
    public class ReportDownloadDto
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
