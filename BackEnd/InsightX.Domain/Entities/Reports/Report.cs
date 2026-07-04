using InsightX.Domain.Enums;

namespace InsightX.Domain.Entities.Reports
{
    public class Report
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public int? DepartmentId { get; set; }

        public string UploadedBy { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public string Status { get; set; } = ReportStatus.Pending.ToString();

        public string ExtractedText { get; set; } = string.Empty;

        // Navigation
        public ICollection<ExtractedMetric> ExtractedMetrics { get; set; }
            = new List<ExtractedMetric>();
    }
}