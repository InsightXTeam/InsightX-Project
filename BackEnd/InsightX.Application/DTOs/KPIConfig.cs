using InsightX.Domain.Enums;

namespace InsightX.Application.DTOs
{
    public class KPIConfig
    {
        public decimal Threshold { get; set; }
        public ThresholdDirection ThresholdDirection { get; set; }
        public decimal AlertPercentageDiff { get; set; }
        public int TrendMonthsCount { get; set; }
    }
}
