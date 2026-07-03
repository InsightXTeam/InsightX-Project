using System;

namespace InsightX.Domain.Entities
{
    public class Alert
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string KPIName { get; set; } = string.Empty; // "Production", "Defect Rate", "Absent Employees", "Revenue"
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public bool SeenByOwner { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
