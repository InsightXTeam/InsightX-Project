using InsightX.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsightX.Domain.Entities.Reports
{
    public class Report
    {
        public int Id { get; set; }

        public string ReportName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public int? DepartmentId { get; set; }

        [Column("UploadedBy")]
        public string UploadedById { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public string Status { get; set; } = ReportStatus.Pending.ToString();

        public string ExtractedText { get; set; } = string.Empty;

        // Navigation
        public ICollection<ExtractedMetric> ExtractedMetrics { get; set; }
            = new List<ExtractedMetric>();

        public Company Company { get; set; } = null!;
        public Department? Department { get; set; }
        
        [ForeignKey("UploadedById")]
        public ApplicationUser UploadedBy { get; set; } = null!;
    }
}