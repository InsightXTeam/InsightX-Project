using InsightX.Domain.Entities.Reports;
using InsightX.Domain.Enums;

namespace InsightX.Domain.Entities
{
    public class Alert
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ReportId { get; set; }
        public string KPIName { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Threshold { get; set; }
        public string Message { get; set; }
        public string Recommendation { get; set; }
        public bool SeenByOwner { get; set; }
        public DateTime CreatedAt { get; set; }
        public AlertType AlertType { get; set; } = AlertType.Anomaly;

        public virtual Company Company { get; set; }
        public virtual Department? Department { get; set; }
        public virtual Report? Report { get; set; }
    }
}
