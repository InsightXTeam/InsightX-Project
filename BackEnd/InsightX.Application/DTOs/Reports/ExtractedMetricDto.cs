namespace InsightX.Application.DTOs.Reports
{
    public class ExtractedMetricDto
    {
        public string KPIName { get; set; } = string.Empty;

        public double? Value { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
    }
}