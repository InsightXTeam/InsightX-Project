namespace InsightX.Domain.Entities
{
    public class ExtractedMetric
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public Report? Report { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public string Month { get; set; } = string.Empty; // "January", "February", "March"
        public int Year { get; set; }
        public string KPIName { get; set; } = string.Empty; // "Production", "Defect Rate", "Absent Employees", "Revenue"
        public double Value { get; set; }
        public bool ConfirmedByManager { get; set; }
    }
}
