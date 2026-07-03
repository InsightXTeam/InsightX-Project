using System;
using System.Collections.Generic;

namespace InsightX.Domain.Entities
{
    public class Report
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        public int UploadedById { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Processing", "Done", "Failed"
        
        public ICollection<ExtractedMetric> ExtractedMetrics { get; set; } = new List<ExtractedMetric>();
    }
}
