namespace InsightX.Application.DTOs.Reports
{
    public class ReportResponseDto
    {
        public int Id { get; set; }

        public string ReportName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        private DateTime _uploadedAt;
        public DateTime UploadedAt
        {
            get => _uploadedAt;
            set => _uploadedAt = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public string UploadedById { get; set; } = string.Empty;

        public string UploadedByName { get; set; } = string.Empty;
    }
}