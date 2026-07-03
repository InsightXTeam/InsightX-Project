namespace InsightX.Domain.Entities
{
    public class KpiThreshold
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public string KPIName { get; set; } = string.Empty; // "Production", "Defect Rate", "Absent Employees", "Revenue"
        public double ThresholdValue { get; set; }
        public string ComparisonType { get; set; } = "Greater"; // "Greater" or "Less"
        public bool IsPercentage { get; set; }
    }
}
