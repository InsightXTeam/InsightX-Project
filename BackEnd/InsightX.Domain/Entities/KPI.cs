// TEMP: will be replaced by Person 1's implementation
using InsightX.Domain.Enums;
namespace InsightX.Domain.Entities
{
    public class KPI
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public decimal Threshold { get; set; }
        public ThresholdDirection ThresholdDirection { get; set; };
        public decimal AlertPercentageDiff { get; set; }
        public int TrendMonthsCount { get; set; }
    }
}
