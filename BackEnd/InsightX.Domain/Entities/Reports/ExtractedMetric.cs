namespace InsightX.Domain.Entities.Reports
{
    public class ExtractedMetric
    {
        public int Id { get; set; }

        public int ReportId { get; set; }

        public int CompanyId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public string KPIName { get; set; } = string.Empty;

        public double? Value { get; set; }

        public bool ConfirmedByManager { get; set; }

        // Navigation
        public Report Report { get; set; } = null!;
    }
}