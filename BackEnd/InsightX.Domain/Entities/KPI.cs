using InsightX.Domain.Enums;

namespace InsightX.Domain.Entities
{
    public class KPI
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Threshold { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal AlertPercentageDiff { get; set; }
        public int TrendMonthsCount { get; set; }
        public ThresholdDirection ThresholdDirection { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}
